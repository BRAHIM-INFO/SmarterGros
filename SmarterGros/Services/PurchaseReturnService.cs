using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmarterGros.Data;
using SmarterGros.Models;
using SmarterGros.Models.Enums;
using SmarterGros.ViewModels;

namespace SmarterGros.Services
{
    /// <summary>
    /// 🔄 خدمة مرتجعات المشتريات - المنطق الأساسي
    /// </summary>
    public class PurchaseReturnService : IPurchaseReturnService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IActivityLogService _activityLogService; 
        private readonly ICashRegisterService _cashRegisterService;

        public PurchaseReturnService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            IActivityLogService activityLogService,
            ICashRegisterService cashRegisterService)  // ⭐ جديد
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _activityLogService = activityLogService;
            _cashRegisterService = cashRegisterService;  // ⭐ جديد
        }

        // ═══════════════════════════════════════════════════
        // 🆕 إنشاء مرتجع
        // ═══════════════════════════════════════════════════

        public async Task<(bool Success, string Message, int? ReturnId)> CreateReturnAsync(
            PurchaseReturnCreateViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ─── التحقق من البيانات ───

                if (model.Items == null || !model.Items.Any(i => i.ReturnedQuantity > 0))
                    return (false, "يجب إرجاع منتج واحد على الأقل بكمية أكبر من 0", null);

                // التحقق من الفاتورة
                var purchase = await _context.Purchases
                    .Include(p => p.PurchaseItems)
                        .ThenInclude(i => i.Product)
                    .Include(p => p.Supplier)
                    .FirstOrDefaultAsync(p => p.Id == model.PurchaseId);

                if (purchase == null)
                    return (false, "الفاتورة غير موجودة", null);

                // التحقق من إمكانية الإرجاع
                if (!purchase.Status.CanReturn())
                    return (false, "لا يمكن إنشاء مرتجع لهذه الفاتورة - يجب أن تكون مستلمة", null);

                // التحقق من طريقة الاسترداد
                if (model.RefundMethod == ReturnRefundMethod.Mixed)
                {
                    if (model.DeductedFromDebt <= 0 && model.CashRefunded <= 0)
                        return (false, "في حالة المزيج، يجب تحديد مبلغ في كل من الخصم والنقدي", null);
                }

                var currentUser = await GetCurrentUserAsync();
                var returnNumber = await GenerateReturnNumberAsync();

                // ─── إنشاء المرتجع ───

                var purchaseReturn = new PurchaseReturn
                {
                    ReturnNumber = returnNumber,
                    ReturnDate = model.ReturnDate,
                    PurchaseId = purchase.Id,
                    SupplierId = purchase.SupplierId,
                    RefundMethod = model.RefundMethod,
                    ReturnReason = model.ReturnReason,
                    Notes = model.Notes,
                    CreatedById = currentUser?.Id,
                    CreatedByName = currentUser?.FullName ?? currentUser?.UserName,
                    CreatedAt = DateTime.Now
                };

                // ─── معالجة البنود ───

                decimal subTotal = 0;
                decimal totalTax = 0;

                foreach (var itemModel in model.Items.Where(i => i.ReturnedQuantity > 0))
                {
                    // البحث عن البند الأصلي
                    var originalItem = purchase.PurchaseItems
                        .FirstOrDefault(pi => pi.Id == itemModel.PurchaseItemId);

                    if (originalItem == null)
                        return (false, $"بند الفاتورة رقم {itemModel.PurchaseItemId} غير موجود", null);

                    if (originalItem.Product == null)
                        return (false, $"المنتج غير موجود", null);

                    // التحقق من الكمية المتاحة للإرجاع
                    var availableForReturn = originalItem.ReceivedQuantity - originalItem.ReturnedQuantity;
                    if (itemModel.ReturnedQuantity > availableForReturn)
                    {
                        return (false,
                            $"الكمية المرتجعة ({itemModel.ReturnedQuantity}) للمنتج '{originalItem.Product.Name}' " +
                            $"أكبر من المتاح للإرجاع ({availableForReturn})", null);
                    }

                    // حسابات البند
                    var itemSubTotal = itemModel.ReturnedQuantity * originalItem.UnitPrice;
                    var itemTax = itemSubTotal * (originalItem.TaxRate / 100);
                    var itemTotal = itemSubTotal + itemTax;

                    var returnItem = new PurchaseReturnItem
                    {
                        PurchaseItemId = originalItem.Id,
                        ProductId = originalItem.ProductId,
                        ReturnedQuantity = itemModel.ReturnedQuantity,
                        UnitPrice = originalItem.UnitPrice,
                        TaxRate = originalItem.TaxRate,
                        TotalPrice = itemTotal,
                        ReturnReason = itemModel.ReturnReason,
                        ProductCondition = itemModel.ProductCondition,
                        BatchNumber = originalItem.BatchNumber
                    };

                    purchaseReturn.ReturnItems.Add(returnItem);

                    subTotal += itemSubTotal;
                    totalTax += itemTax;

                    // ─── تأثير المخزون (نقص!) ───

                    var oldStock = originalItem.Product.StockQuantity;
                    originalItem.Product.StockQuantity -= itemModel.ReturnedQuantity;

                    if (originalItem.Product.StockQuantity < 0)
                        originalItem.Product.StockQuantity = 0;

                    // تحديث ReturnedQuantity في البند الأصلي
                    originalItem.ReturnedQuantity += itemModel.ReturnedQuantity;

                    // تسجيل حركة المخزون
                    _context.StockMovements.Add(new StockMovement
                    {
                        ProductId = originalItem.ProductId,
                        MovementType = "إخراج - مرتجع شراء",
                        Quantity = -itemModel.ReturnedQuantity,
                        QuantityBefore = oldStock,
                        QuantityAfter = originalItem.Product.StockQuantity,
                        Reason = $"مرتجع شراء {returnNumber}",
                        UserId = currentUser?.Id,
                        UserName = currentUser?.FullName ?? currentUser?.UserName,
                        MovementDate = model.ReturnDate,
                        Notes = itemModel.ReturnReason
                    });
                }

                // ─── حساب المجاميع ───

                purchaseReturn.SubTotal = subTotal;
                purchaseReturn.TaxAmount = totalTax;
                purchaseReturn.TotalAmount = subTotal + totalTax;

                // ─── معالجة طريقة الاسترداد ───

                switch (model.RefundMethod)
                {
                    case ReturnRefundMethod.DeductFromDebt:
                        purchaseReturn.DeductedFromDebt = purchaseReturn.TotalAmount;
                        purchaseReturn.CashRefunded = 0;
                        break;

                    case ReturnRefundMethod.CashRefund:
                        purchaseReturn.DeductedFromDebt = 0;
                        purchaseReturn.CashRefunded = purchaseReturn.TotalAmount;
                        break;

                    case ReturnRefundMethod.Mixed:
                        // التحقق من أن المجموع صحيح
                        if (model.DeductedFromDebt + model.CashRefunded != purchaseReturn.TotalAmount)
                        {
                            return (false,
                                $"مجموع المبالغ ({model.DeductedFromDebt + model.CashRefunded:N2}) " +
                                $"لا يساوي مبلغ المرتجع ({purchaseReturn.TotalAmount:N2})", null);
                        }
                        purchaseReturn.DeductedFromDebt = model.DeductedFromDebt;
                        purchaseReturn.CashRefunded = model.CashRefunded;
                        break;
                }

                // ─── تأثير على الدين (إذا خصم من الدين) ───

                if (purchaseReturn.DeductedFromDebt > 0 && purchase.RemainingAmount > 0)
                {
                    var deductAmount = Math.Min(purchaseReturn.DeductedFromDebt, purchase.RemainingAmount);
                    purchase.RemainingAmount -= deductAmount;
                    purchase.UpdatedAt = DateTime.Now;
                }

                // ─── تأثير على الصندوق (إذا استرداد نقدي) ───
                // ملاحظة: يمكن إضافة جدول CashMovements لاحقاً
                // الآن نسجل فقط في Activity Log

                // ─── الحفظ ───

                _context.PurchaseReturns.Add(purchaseReturn);
                await _context.SaveChangesAsync();

                // ✅ التكامل مع الصندوق - إضافة الاسترداد النقدي
                if (purchaseReturn.CashRefunded > 0)
                {
                    await _cashRegisterService.RecordPurchaseRefundAsync(
                        returnId: purchaseReturn.Id,
                        returnNumber: purchaseReturn.ReturnNumber,
                        amount: purchaseReturn.CashRefunded,
                        supplierId: purchase.SupplierId,
                        supplierName: purchase.Supplier?.Name ?? "غير معروف",
                        notes: $"استرداد نقدي من مرتجع {purchaseReturn.ReturnNumber}"
                    );
                }

                // ─── Activity Log ───

                await _activityLogService.LogCreateAsync(
                    module: "PurchaseReturns",
                    entityName: "PurchaseReturn",
                    entityId: purchaseReturn.Id,
                    description: $"إنشاء مرتجع {returnNumber} للفاتورة {purchase.InvoiceNumber} - " +
                                 $"المبلغ: {purchaseReturn.TotalAmount:N2} دج - " +
                                 $"طريقة الاسترداد: {model.RefundMethod.GetArabicName()}",
                    newValues: new
                    {
                        purchaseReturn.ReturnNumber,
                        PurchaseInvoice = purchase.InvoiceNumber,
                        SupplierName = purchase.Supplier?.Name,
                        purchaseReturn.TotalAmount,
                        purchaseReturn.RefundMethod,
                        ItemsCount = purchaseReturn.ReturnItems.Count
                    });

                await transaction.CommitAsync();

                return (true,
                    $"تم إنشاء المرتجع {returnNumber} بنجاح - " +
                    $"المبلغ: {purchaseReturn.TotalAmount:N2} دج",
                    purchaseReturn.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _activityLogService.LogErrorAsync(
                    actionName: "إنشاء مرتجع شراء",
                    errorMessage: ex.Message,
                    module: "PurchaseReturns");
                return (false, $"حدث خطأ: {ex.Message}", null);
            }
        }

        // ═══════════════════════════════════════════════════
        // ❌ إلغاء مرتجع (عكس التأثيرات)
        // ═══════════════════════════════════════════════════

        public async Task<(bool Success, string Message)> CancelReturnAsync(
            int returnId, string cancellationReason)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (string.IsNullOrWhiteSpace(cancellationReason))
                    return (false, "سبب الإلغاء مطلوب");

                var purchaseReturn = await _context.PurchaseReturns
                    .Include(r => r.ReturnItems)
                        .ThenInclude(i => i.Product)
                    .Include(r => r.ReturnItems)
                        .ThenInclude(i => i.PurchaseItem)
                    .Include(r => r.Purchase)
                    .Include(r => r.Supplier)
                    .FirstOrDefaultAsync(r => r.Id == returnId);

                if (purchaseReturn == null)
                    return (false, "المرتجع غير موجود");

                if (purchaseReturn.IsCancelled)
                    return (false, "المرتجع ملغى بالفعل");

                var currentUser = await GetCurrentUserAsync();

                // ─── عكس تأثير المخزون (إرجاع الكميات!) ───

                foreach (var returnItem in purchaseReturn.ReturnItems)
                {
                    if (returnItem.Product == null || returnItem.PurchaseItem == null)
                        continue;

                    var oldStock = returnItem.Product.StockQuantity;
                    returnItem.Product.StockQuantity += returnItem.ReturnedQuantity;

                    // تحديث ReturnedQuantity في البند الأصلي
                    returnItem.PurchaseItem.ReturnedQuantity -= returnItem.ReturnedQuantity;
                    if (returnItem.PurchaseItem.ReturnedQuantity < 0)
                        returnItem.PurchaseItem.ReturnedQuantity = 0;

                    // تسجيل حركة المخزون (عكسية)
                    _context.StockMovements.Add(new StockMovement
                    {
                        ProductId = returnItem.ProductId,
                        MovementType = "إدخال - إلغاء مرتجع",
                        Quantity = returnItem.ReturnedQuantity,
                        QuantityBefore = oldStock,
                        QuantityAfter = returnItem.Product.StockQuantity,
                        Reason = $"إلغاء مرتجع {purchaseReturn.ReturnNumber}",
                        UserId = currentUser?.Id,
                        UserName = currentUser?.FullName ?? currentUser?.UserName,
                        MovementDate = DateTime.Now,
                        Notes = $"السبب: {cancellationReason}"
                    });
                }

                // ─── عكس تأثير الدين ───

                if (purchaseReturn.DeductedFromDebt > 0 && purchaseReturn.Purchase != null)
                {
                    purchaseReturn.Purchase.RemainingAmount += purchaseReturn.DeductedFromDebt;
                    purchaseReturn.Purchase.UpdatedAt = DateTime.Now;
                }

                // ─── تحديث المرتجع ───

                purchaseReturn.IsCancelled = true;
                purchaseReturn.CancellationReason = cancellationReason;
                purchaseReturn.CancelledAt = DateTime.Now;
                purchaseReturn.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                // Activity Log
                await _activityLogService.LogAsync(
                    actionType: "Cancel",
                    actionName: "إلغاء مرتجع شراء",
                    module: "PurchaseReturns",
                    entityName: "PurchaseReturn",
                    entityId: purchaseReturn.Id,
                    description: $"إلغاء المرتجع {purchaseReturn.ReturnNumber} - السبب: {cancellationReason}",
                    severity: "Critical");

                await transaction.CommitAsync();
                return (true,
                    $"تم إلغاء المرتجع {purchaseReturn.ReturnNumber} بنجاح " +
                    $"وتم عكس تأثيرات المخزون والدين");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _activityLogService.LogErrorAsync(
                    actionName: "إلغاء مرتجع شراء",
                    errorMessage: ex.Message,
                    module: "PurchaseReturns");
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════
        // 🗑️ حذف مرتجع (للأدمن فقط - خطير!)
        // ═══════════════════════════════════════════════════

        public async Task<(bool Success, string Message)> DeleteReturnAsync(int returnId)
        {
            try
            {
                var purchaseReturn = await _context.PurchaseReturns
                    .Include(r => r.ReturnItems)
                    .FirstOrDefaultAsync(r => r.Id == returnId);

                if (purchaseReturn == null)
                    return (false, "المرتجع غير موجود");

                if (!purchaseReturn.IsCancelled)
                    return (false, "لا يمكن حذف مرتجع نشط - يجب إلغاؤه أولاً");

                var deletedData = new
                {
                    purchaseReturn.ReturnNumber,
                    purchaseReturn.TotalAmount,
                    ItemsCount = purchaseReturn.ReturnItems.Count
                };

                _context.PurchaseReturnItems.RemoveRange(purchaseReturn.ReturnItems);
                _context.PurchaseReturns.Remove(purchaseReturn);
                await _context.SaveChangesAsync();

                await _activityLogService.LogDeleteAsync(
                    module: "PurchaseReturns",
                    entityName: "PurchaseReturn",
                    entityId: returnId,
                    description: $"حذف المرتجع {purchaseReturn.ReturnNumber}",
                    deletedData: deletedData);

                return (true, "تم حذف المرتجع بنجاح");
            }
            catch (Exception ex)
            {
                await _activityLogService.LogErrorAsync(
                    actionName: "حذف مرتجع شراء",
                    errorMessage: ex.Message,
                    module: "PurchaseReturns");
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════
        // 🔍 الاستعلامات
        // ═══════════════════════════════════════════════════

        public async Task<PurchaseReturnListViewModel> GetReturnsAsync(
            string? search = null,
            int? supplierId = null,
            ReturnRefundMethod? refundMethod = null,
            bool? isCancelled = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.PurchaseReturns
                .Include(r => r.Supplier)
                .Include(r => r.Purchase)
                .Include(r => r.ReturnItems)
                .AsQueryable();

            // الفلاتر
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r =>
                    r.ReturnNumber.Contains(search) ||
                    (r.Supplier != null && r.Supplier.Name.Contains(search)) ||
                    (r.Purchase != null && r.Purchase.InvoiceNumber.Contains(search)));
            }

            if (supplierId.HasValue)
                query = query.Where(r => r.SupplierId == supplierId.Value);

            if (refundMethod.HasValue)
                query = query.Where(r => r.RefundMethod == refundMethod.Value);

            if (isCancelled.HasValue)
                query = query.Where(r => r.IsCancelled == isCancelled.Value);

            if (dateFrom.HasValue)
                query = query.Where(r => r.ReturnDate >= dateFrom.Value);

            if (dateTo.HasValue)
            {
                var endDate = dateTo.Value.Date.AddDays(1);
                query = query.Where(r => r.ReturnDate < endDate);
            }

            // العدد
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Pagination
            var returns = await query
                .OrderByDescending(r => r.ReturnDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // الإحصائيات
            var allReturns = await _context.PurchaseReturns.ToListAsync();
            var activeReturns = allReturns.Where(r => !r.IsCancelled).ToList();

            // الموردين
            var suppliers = await _context.Suppliers
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return new PurchaseReturnListViewModel
            {
                Returns = returns,
                Suppliers = suppliers,
                SearchTerm = search,
                SupplierId = supplierId,
                RefundMethod = refundMethod,
                IsCancelled = isCancelled,
                DateFrom = dateFrom,
                DateTo = dateTo,
                TotalCount = totalCount,
                TotalAmount = activeReturns.Sum(r => r.TotalAmount),
                TotalDeductedFromDebt = activeReturns.Sum(r => r.DeductedFromDebt),
                TotalCashRefunded = activeReturns.Sum(r => r.CashRefunded),
                CancelledCount = allReturns.Count(r => r.IsCancelled),
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<PurchaseReturnDetailsViewModel?> GetReturnDetailsAsync(int returnId)
        {
            var purchaseReturn = await _context.PurchaseReturns
                .Include(r => r.Supplier)
                .Include(r => r.Purchase)
                    .ThenInclude(p => p!.Supplier)
                .Include(r => r.ReturnItems)
                    .ThenInclude(i => i.Product)
                .Include(r => r.ReturnItems)
                    .ThenInclude(i => i.PurchaseItem)
                .FirstOrDefaultAsync(r => r.Id == returnId);

            if (purchaseReturn == null) return null;

            // الفاتورة الأصلية الكاملة
            var originalPurchase = await _context.Purchases
                .Include(p => p.PurchaseItems)
                    .ThenInclude(i => i.Product)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.Id == purchaseReturn.PurchaseId);

            if (originalPurchase == null) return null;

            // Activity Logs
            var activityLogs = await _activityLogService.GetEntityHistoryAsync(
                "PurchaseReturn", returnId, 20);

            return new PurchaseReturnDetailsViewModel
            {
                PurchaseReturn = purchaseReturn,
                OriginalPurchase = originalPurchase,
                ActivityLogs = activityLogs
            };
        }

        public async Task<PurchaseReturnCreateViewModel?> GetReturnFormForPurchaseAsync(int purchaseId)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(p => p.Id == purchaseId);

            if (purchase == null) return null;

            // التحقق من إمكانية الإرجاع
            if (!purchase.Status.CanReturn()) return null;

            return new PurchaseReturnCreateViewModel
            {
                PurchaseId = purchase.Id,
                InvoiceNumber = purchase.InvoiceNumber,
                SupplierId = purchase.SupplierId,
                SupplierName = purchase.Supplier?.Name,
                ReturnDate = DateTime.Now,
                RefundMethod = purchase.RemainingAmount > 0
                    ? ReturnRefundMethod.DeductFromDebt
                    : ReturnRefundMethod.CashRefund,
                Items = purchase.PurchaseItems
                    .Where(i => i.AvailableForReturn > 0)  // فقط البنود القابلة للإرجاع
                    .Select(i => new PurchaseReturnItemCreateViewModel
                    {
                        PurchaseItemId = i.Id,
                        ProductId = i.ProductId,
                        ProductName = i.Product?.Name ?? "",
                        ReceivedQuantity = i.ReceivedQuantity,
                        PreviouslyReturned = i.ReturnedQuantity,
                        ReturnedQuantity = 0,  // افتراضياً صفر
                        UnitPrice = i.UnitPrice,
                        TaxRate = i.TaxRate,
                        BatchNumber = i.BatchNumber
                    }).ToList()
            };
        }

        // ═══════════════════════════════════════════════════
        // 🛠️ Helpers
        // ═══════════════════════════════════════════════════

        public async Task<string> GenerateReturnNumberAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"RET-{year}-";

            var lastNumber = await _context.PurchaseReturns
                .Where(r => r.ReturnNumber.StartsWith(prefix))
                .OrderByDescending(r => r.ReturnNumber)
                .Select(r => r.ReturnNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastNumber))
            {
                var numberPart = lastNumber.Substring(prefix.Length);
                if (int.TryParse(numberPart, out int parsed))
                    nextNumber = parsed + 1;
            }

            return $"{prefix}{nextNumber:D5}";
        }

        public async Task<(bool CanReturn, string Reason)> CanCreateReturnForPurchaseAsync(int purchaseId)
        {
            var purchase = await _context.Purchases
                .Include(p => p.PurchaseItems)
                .FirstOrDefaultAsync(p => p.Id == purchaseId);

            if (purchase == null)
                return (false, "الفاتورة غير موجودة");

            if (!purchase.Status.CanReturn())
                return (false, "يمكن إنشاء مرتجع فقط للفواتير المستلمة");

            // التحقق من وجود كميات متاحة للإرجاع
            var hasAvailableQty = purchase.PurchaseItems.Any(i => i.AvailableForReturn > 0);
            if (!hasAvailableQty)
                return (false, "لا توجد كميات متاحة للإرجاع - جميع المنتجات تم إرجاعها مسبقاً");

            return (true, "");
        }

        public async Task<bool> CanCancelReturnAsync(int returnId)
        {
            var purchaseReturn = await _context.PurchaseReturns.FindAsync(returnId);
            return purchaseReturn != null && !purchaseReturn.IsCancelled;
        }

        // ═══════════════════════════════════════════════════
        // 🔧 Private Helpers
        // ═══════════════════════════════════════════════════

        private async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
                return await _userManager.GetUserAsync(user);
            return null;
        }
    }
}