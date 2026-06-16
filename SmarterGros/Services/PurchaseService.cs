using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmarterGros.Data;
using SmarterGros.Models;
using SmarterGros.Models.Enums;
using SmarterGros.ViewModels;

namespace SmarterGros.Services
{
    /// <summary>
    /// 🛒 خدمة المشتريات - المنطق الأساسي
    /// </summary>
    public class PurchaseService : IPurchaseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IActivityLogService _activityLogService;

        public PurchaseService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            IActivityLogService activityLogService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _activityLogService = activityLogService;
        }

        // ═══════════════════════════════════════════════════
        // 🆕 إنشاء فاتورة شراء
        // ═══════════════════════════════════════════════════

        public async Task<(bool Success, string Message, int? PurchaseId)> CreatePurchaseAsync(
            PurchaseCreateViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // التحقق من البيانات
                if (model.Items == null || !model.Items.Any())
                    return (false, "يجب إضافة منتج واحد على الأقل", null);

                var supplier = await _context.Suppliers.FindAsync(model.SupplierId);
                if (supplier == null)
                    return (false, "المورد غير موجود", null);

                // الحصول على المستخدم الحالي
                var currentUser = await GetCurrentUserAsync();

                // توليد رقم الفاتورة
                var invoiceNumber = await GenerateInvoiceNumberAsync();

                // إنشاء الفاتورة
                var purchase = new Purchase
                {
                    InvoiceNumber = invoiceNumber,
                    SupplierInvoiceNumber = model.SupplierInvoiceNumber,
                    SupplierId = model.SupplierId,
                    PurchaseDate = model.PurchaseDate,
                    Status = model.SaveAsDraft ? InvoiceStatus.Draft : InvoiceStatus.Sent,
                    PaymentType = model.PaymentType,
                    DiscountPercentage = model.DiscountPercentage,
                    TransporterName = model.TransporterName,
                    TransporterPhone = model.TransporterPhone,
                    DeliveryNoteNumber = model.DeliveryNoteNumber,
                    ShippingCost = model.ShippingCost,
                    ShippingDate = model.ShippingDate,
                    ShippingStatus = model.ShippingStatus,
                    Notes = model.Notes,
                    CreatedById = currentUser?.Id,
                    CreatedByName = currentUser?.FullName ?? currentUser?.UserName,
                    CreatedAt = DateTime.Now
                };

                // إضافة البنود وحساب المجاميع
                decimal subTotal = 0;
                decimal totalTax = 0;

                foreach (var itemModel in model.Items)
                {
                    var product = await _context.Products.FindAsync(itemModel.ProductId);
                    if (product == null)
                        return (false, $"المنتج رقم {itemModel.ProductId} غير موجود", null);

                    var itemSubTotal = itemModel.Quantity * itemModel.UnitPrice;
                    var itemDiscount = itemSubTotal * (itemModel.Discount / 100);
                    var itemAfterDiscount = itemSubTotal - itemDiscount;
                    var itemTax = itemAfterDiscount * (itemModel.TaxRate / 100);
                    var itemTotal = itemAfterDiscount + itemTax;

                    var purchaseItem = new PurchaseItem
                    {
                        ProductId = itemModel.ProductId,
                        Quantity = itemModel.Quantity,
                        UnitPrice = itemModel.UnitPrice,
                        Discount = itemModel.Discount,
                        TaxRate = itemModel.TaxRate,
                        TotalPrice = itemTotal,
                        Notes = itemModel.Notes,
                        BatchNumber = itemModel.BatchNumber,
                        BatchExpiryDate = itemModel.BatchExpiryDate
                    };

                    purchase.PurchaseItems.Add(purchaseItem);

                    subTotal += itemAfterDiscount;
                    totalTax += itemTax;
                }

                // حساب المجاميع
                purchase.SubTotal = subTotal;
                purchase.TaxAmount = totalTax;
                purchase.Discount = model.Discount;
                purchase.TotalAmount = subTotal + totalTax - model.Discount;

                // حساب الدفع
                CalculatePayment(purchase, model.PaymentType, model.PaidAmount);

                // حفظ الفاتورة
                _context.Purchases.Add(purchase);
                await _context.SaveChangesAsync();

                // تسجيل الدفعة في SupplierPayments (إذا Cash أو Partial)
                if (purchase.PaidAmount > 0)
                {
                    var payment = new SupplierPayment
                    {
                        SupplierId = purchase.SupplierId,
                        PurchaseId = purchase.Id,
                        Amount = purchase.PaidAmount,
                        PaymentDate = purchase.PurchaseDate,
                        Notes = $"دفعة عند إنشاء الفاتورة {purchase.InvoiceNumber}",
                        CreatedAt = DateTime.Now
                    };
                    _context.SupplierPayments.Add(payment);
                    await _context.SaveChangesAsync();
                }

                // تسجيل في Activity Log
                await _activityLogService.LogCreateAsync(
                    module: "Purchases",
                    entityName: "Purchase",
                    entityId: purchase.Id,
                    description: $"إنشاء فاتورة شراء {purchase.InvoiceNumber} - المورد: {supplier.Name} - المبلغ: {purchase.TotalAmount:N2} دج",
                    newValues: new
                    {
                        purchase.InvoiceNumber,
                        SupplierName = supplier.Name,
                        purchase.TotalAmount,
                        purchase.PaymentType,
                        purchase.Status
                    });

                await transaction.CommitAsync();
                return (true, $"تم إنشاء الفاتورة {invoiceNumber} بنجاح", purchase.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _activityLogService.LogErrorAsync(
                    actionName: "إنشاء فاتورة شراء",
                    errorMessage: ex.Message,
                    module: "Purchases");
                return (false, $"حدث خطأ: {ex.Message}", null);
            }
        }

        // ═══════════════════════════════════════════════════
        // ✏️ تعديل فاتورة (مسودة فقط)
        // ═══════════════════════════════════════════════════

        public async Task<(bool Success, string Message)> UpdatePurchaseAsync(
            int id, PurchaseCreateViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var purchase = await _context.Purchases
                    .Include(p => p.PurchaseItems)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (purchase == null)
                    return (false, "الفاتورة غير موجودة");

                if (!purchase.Status.CanEdit())
                    return (false, "لا يمكن تعديل هذه الفاتورة في حالتها الحالية");

                // حفظ القيم القديمة للسجل
                var oldValues = new
                {
                    purchase.InvoiceNumber,
                    purchase.SupplierId,
                    purchase.TotalAmount
                };

                // حذف البنود القديمة
                _context.PurchaseItems.RemoveRange(purchase.PurchaseItems);

                // تحديث بيانات الفاتورة
                purchase.SupplierId = model.SupplierId;
                purchase.SupplierInvoiceNumber = model.SupplierInvoiceNumber;
                purchase.PurchaseDate = model.PurchaseDate;
                purchase.PaymentType = model.PaymentType;
                purchase.DiscountPercentage = model.DiscountPercentage;
                purchase.TransporterName = model.TransporterName;
                purchase.TransporterPhone = model.TransporterPhone;
                purchase.DeliveryNoteNumber = model.DeliveryNoteNumber;
                purchase.ShippingCost = model.ShippingCost;
                purchase.ShippingDate = model.ShippingDate;
                purchase.ShippingStatus = model.ShippingStatus;
                purchase.Notes = model.Notes;
                purchase.UpdatedAt = DateTime.Now;
                purchase.PurchaseItems = new List<PurchaseItem>();

                // إضافة البنود الجديدة
                decimal subTotal = 0;
                decimal totalTax = 0;

                foreach (var itemModel in model.Items)
                {
                    var itemSubTotal = itemModel.Quantity * itemModel.UnitPrice;
                    var itemDiscount = itemSubTotal * (itemModel.Discount / 100);
                    var itemAfterDiscount = itemSubTotal - itemDiscount;
                    var itemTax = itemAfterDiscount * (itemModel.TaxRate / 100);
                    var itemTotal = itemAfterDiscount + itemTax;

                    var purchaseItem = new PurchaseItem
                    {
                        PurchaseId = purchase.Id,
                        ProductId = itemModel.ProductId,
                        Quantity = itemModel.Quantity,
                        UnitPrice = itemModel.UnitPrice,
                        Discount = itemModel.Discount,
                        TaxRate = itemModel.TaxRate,
                        TotalPrice = itemTotal,
                        Notes = itemModel.Notes,
                        BatchNumber = itemModel.BatchNumber,
                        BatchExpiryDate = itemModel.BatchExpiryDate
                    };

                    purchase.PurchaseItems.Add(purchaseItem);

                    subTotal += itemAfterDiscount;
                    totalTax += itemTax;
                }

                purchase.SubTotal = subTotal;
                purchase.TaxAmount = totalTax;
                purchase.Discount = model.Discount;
                purchase.TotalAmount = subTotal + totalTax - model.Discount;

                CalculatePayment(purchase, model.PaymentType, model.PaidAmount);

                await _context.SaveChangesAsync();

                // Activity Log
                await _activityLogService.LogUpdateAsync(
                    module: "Purchases",
                    entityName: "Purchase",
                    entityId: purchase.Id,
                    description: $"تعديل الفاتورة {purchase.InvoiceNumber}",
                    oldValues: oldValues,
                    newValues: new
                    {
                        purchase.InvoiceNumber,
                        purchase.SupplierId,
                        purchase.TotalAmount
                    });

                await transaction.CommitAsync();
                return (true, "تم تعديل الفاتورة بنجاح");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _activityLogService.LogErrorAsync(
                    actionName: "تعديل فاتورة شراء",
                    errorMessage: ex.Message,
                    module: "Purchases");
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════
        // 🗑️ حذف فاتورة (مسودة فقط)
        // ═══════════════════════════════════════════════════

        public async Task<(bool Success, string Message)> DeletePurchaseAsync(int id)
        {
            try
            {
                var purchase = await _context.Purchases
                    .Include(p => p.PurchaseItems)
                    .Include(p => p.Supplier)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (purchase == null)
                    return (false, "الفاتورة غير موجودة");

                if (!purchase.Status.CanDelete())
                    return (false, "لا يمكن حذف هذه الفاتورة - فقط المسودات يمكن حذفها");

                var deletedData = new
                {
                    purchase.InvoiceNumber,
                    SupplierName = purchase.Supplier?.Name,
                    purchase.TotalAmount,
                    ItemsCount = purchase.PurchaseItems.Count
                };

                _context.PurchaseItems.RemoveRange(purchase.PurchaseItems);
                _context.Purchases.Remove(purchase);
                await _context.SaveChangesAsync();

                await _activityLogService.LogDeleteAsync(
                    module: "Purchases",
                    entityName: "Purchase",
                    entityId: id,
                    description: $"حذف الفاتورة {purchase.InvoiceNumber}",
                    deletedData: deletedData);

                return (true, "تم حذف الفاتورة بنجاح");
            }
            catch (Exception ex)
            {
                await _activityLogService.LogErrorAsync(
                    actionName: "حذف فاتورة شراء",
                    errorMessage: ex.Message,
                    module: "Purchases");
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════
        // 📦 استلام الفاتورة (تأثير المخزون!)
        // ═══════════════════════════════════════════════════

        public async Task<(bool Success, string Message)> ReceivePurchaseAsync(
            PurchaseReceiveViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var purchase = await _context.Purchases
                    .Include(p => p.PurchaseItems)
                        .ThenInclude(i => i.Product)
                    .Include(p => p.Supplier)
                    .FirstOrDefaultAsync(p => p.Id == model.PurchaseId);

                if (purchase == null)
                    return (false, "الفاتورة غير موجودة");

                if (!purchase.Status.CanReceive())
                    return (false, "لا يمكن استلام هذه الفاتورة في حالتها الحالية");

                var currentUser = await GetCurrentUserAsync();

                // معالجة كل بند
                foreach (var itemModel in model.Items)
                {
                    var item = purchase.PurchaseItems.FirstOrDefault(
                        i => i.Id == itemModel.PurchaseItemId);

                    if (item == null || item.Product == null)
                        continue;

                    // تحديث الكمية المستلمة
                    item.ReceivedQuantity = itemModel.ReceivedQuantity;
                    if (!string.IsNullOrEmpty(itemModel.Notes))
                        item.Notes = itemModel.Notes;

                    // تأثير المخزون (إضافة)
                    var oldStock = item.Product.StockQuantity;
                    item.Product.StockQuantity += itemModel.ReceivedQuantity;

                    // تحديث سعر التكلفة إذا طُلب
                    if (model.UpdateProductPrices)
                    {
                        item.Product.PurchasePriceHT = item.UnitPrice;
                        item.Product.PurchasePriceTTC = item.UnitPrice * (1 + item.TaxRate / 100);
                    }

                    // تسجيل حركة المخزون
                    _context.StockMovements.Add(new StockMovement
                    {
                        ProductId = item.ProductId,
                        MovementType = "إدخال - شراء",
                        Quantity = itemModel.ReceivedQuantity,
                        QuantityBefore = oldStock,
                        QuantityAfter = item.Product.StockQuantity,
                        Reason = $"استلام فاتورة شراء {purchase.InvoiceNumber}",
                        UserId = currentUser?.Id,
                        UserName = currentUser?.FullName ?? currentUser?.UserName,
                        MovementDate = model.ReceivedDate,
                        Notes = itemModel.Notes
                    });
                }

                // تحديث الفاتورة
                purchase.Status = InvoiceStatus.Received;
                purchase.ReceivedDate = model.ReceivedDate;
                purchase.ShippingStatus = model.ShippingStatus;
                purchase.ReceivedById = currentUser?.Id;
                purchase.ReceivedByName = currentUser?.FullName ?? currentUser?.UserName;
                purchase.UpdatedAt = DateTime.Now;

                if (!string.IsNullOrEmpty(model.ReceivingNotes))
                {
                    purchase.Notes = string.IsNullOrEmpty(purchase.Notes)
                        ? $"ملاحظات الاستلام: {model.ReceivingNotes}"
                        : $"{purchase.Notes}\n\nملاحظات الاستلام: {model.ReceivingNotes}";
                }

                await _context.SaveChangesAsync();

                // Activity Log
                await _activityLogService.LogAsync(
                    actionType: "Receive",
                    actionName: "استلام فاتورة شراء",
                    module: "Purchases",
                    entityName: "Purchase",
                    entityId: purchase.Id,
                    description: $"تم استلام الفاتورة {purchase.InvoiceNumber} - المورد: {purchase.Supplier?.Name}",
                    severity: "Info");

                await transaction.CommitAsync();
                return (true, $"تم استلام الفاتورة {purchase.InvoiceNumber} بنجاح وتحديث المخزون");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _activityLogService.LogErrorAsync(
                    actionName: "استلام فاتورة شراء",
                    errorMessage: ex.Message,
                    module: "Purchases");
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════
        // ❌ إلغاء فاتورة (عكس التأثيرات!)
        // ═══════════════════════════════════════════════════

        public async Task<(bool Success, string Message)> CancelPurchaseAsync(
            PurchaseCancelViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var purchase = await _context.Purchases
                    .Include(p => p.PurchaseItems)
                        .ThenInclude(i => i.Product)
                    .Include(p => p.Supplier)
                    .Include(p => p.Returns)
                    .FirstOrDefaultAsync(p => p.Id == model.PurchaseId);

                if (purchase == null)
                    return (false, "الفاتورة غير موجودة");

                if (!purchase.Status.CanCancel())
                    return (false, "لا يمكن إلغاء هذه الفاتورة");

                if (purchase.Returns != null && purchase.Returns.Any(r => !r.IsCancelled))
                    return (false, "لا يمكن إلغاء الفاتورة - لها مرتجعات نشطة. ألغِ المرتجعات أولاً");

                var currentUser = await GetCurrentUserAsync();
                var wasReceived = purchase.Status == InvoiceStatus.Received;

                // إذا كانت مستلمة، نعكس التأثيرات
                if (wasReceived)
                {
                    foreach (var item in purchase.PurchaseItems)
                    {
                        if (item.Product == null) continue;

                        var oldStock = item.Product.StockQuantity;
                        var qtyToRemove = item.ReceivedQuantity - item.ReturnedQuantity;

                        item.Product.StockQuantity -= qtyToRemove;
                        if (item.Product.StockQuantity < 0)
                            item.Product.StockQuantity = 0;

                        // تسجيل حركة المخزون (عكسية)
                        _context.StockMovements.Add(new StockMovement
                        {
                            ProductId = item.ProductId,
                            MovementType = "إخراج - إلغاء شراء",
                            Quantity = -qtyToRemove,
                            QuantityBefore = oldStock,
                            QuantityAfter = item.Product.StockQuantity,
                            Reason = $"إلغاء فاتورة شراء {purchase.InvoiceNumber}",
                            UserId = currentUser?.Id,
                            UserName = currentUser?.FullName ?? currentUser?.UserName,
                            MovementDate = DateTime.Now,
                            Notes = $"السبب: {model.CancellationReason}"
                        });
                    }
                }

                // تحديث حالة الفاتورة
                purchase.Status = InvoiceStatus.Cancelled;
                purchase.CancellationReason = model.CancellationReason;
                purchase.CancelledAt = DateTime.Now;
                purchase.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                // Activity Log (Critical لأنه عكس)
                await _activityLogService.LogAsync(
                    actionType: "Cancel",
                    actionName: "إلغاء فاتورة شراء",
                    module: "Purchases",
                    entityName: "Purchase",
                    entityId: purchase.Id,
                    description: $"إلغاء الفاتورة {purchase.InvoiceNumber} - السبب: {model.CancellationReason}",
                    severity: "Critical");

                await transaction.CommitAsync();
                return (true, $"تم إلغاء الفاتورة {purchase.InvoiceNumber} بنجاح" +
                    (wasReceived ? " وتم عكس تأثيرات المخزون" : ""));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _activityLogService.LogErrorAsync(
                    actionName: "إلغاء فاتورة شراء",
                    errorMessage: ex.Message,
                    module: "Purchases");
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════
        // 📋 نسخ فاتورة (Duplicate)
        // ═══════════════════════════════════════════════════

        public async Task<(bool Success, string Message, int? NewPurchaseId)> DuplicatePurchaseAsync(
            int sourceId)
        {
            try
            {
                var source = await _context.Purchases
                    .Include(p => p.PurchaseItems)
                    .FirstOrDefaultAsync(p => p.Id == sourceId);

                if (source == null)
                    return (false, "الفاتورة المصدر غير موجودة", null);

                var currentUser = await GetCurrentUserAsync();
                var invoiceNumber = await GenerateInvoiceNumberAsync();

                var newPurchase = new Purchase
                {
                    InvoiceNumber = invoiceNumber,
                    SupplierId = source.SupplierId,
                    PurchaseDate = DateTime.Now,
                    Status = InvoiceStatus.Draft,
                    PaymentType = PaymentType.Cash,
                    SubTotal = source.SubTotal,
                    TaxAmount = source.TaxAmount,
                    Discount = source.Discount,
                    DiscountPercentage = source.DiscountPercentage,
                    TotalAmount = source.TotalAmount,
                    ShippingCost = source.ShippingCost,
                    Notes = $"نسخة من الفاتورة {source.InvoiceNumber}",
                    CreatedById = currentUser?.Id,
                    CreatedByName = currentUser?.FullName ?? currentUser?.UserName,
                    CreatedAt = DateTime.Now
                };

                // نسخ البنود
                foreach (var sourceItem in source.PurchaseItems)
                {
                    newPurchase.PurchaseItems.Add(new PurchaseItem
                    {
                        ProductId = sourceItem.ProductId,
                        Quantity = sourceItem.Quantity,
                        UnitPrice = sourceItem.UnitPrice,
                        Discount = sourceItem.Discount,
                        TaxRate = sourceItem.TaxRate,
                        TotalPrice = sourceItem.TotalPrice
                    });
                }

                _context.Purchases.Add(newPurchase);
                await _context.SaveChangesAsync();

                await _activityLogService.LogCreateAsync(
                    module: "Purchases",
                    entityName: "Purchase",
                    entityId: newPurchase.Id,
                    description: $"نسخ فاتورة: {source.InvoiceNumber} → {newPurchase.InvoiceNumber}");

                return (true, $"تم نسخ الفاتورة بنجاح برقم {invoiceNumber}", newPurchase.Id);
            }
            catch (Exception ex)
            {
                return (false, $"حدث خطأ: {ex.Message}", null);
            }
        }

        // ═══════════════════════════════════════════════════
        // 💳 تسجيل دفعة
        // ═══════════════════════════════════════════════════

        public async Task<(bool Success, string Message)> RegisterPaymentAsync(
            SupplierPaymentViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var supplier = await _context.Suppliers.FindAsync(model.SupplierId);
                if (supplier == null)
                    return (false, "المورد غير موجود");

                Purchase? purchase = null;
                if (model.PurchaseId.HasValue)
                {
                    purchase = await _context.Purchases.FindAsync(model.PurchaseId.Value);
                    if (purchase == null)
                        return (false, "الفاتورة غير موجودة");

                    if (model.Amount > purchase.RemainingAmount)
                        return (false, $"المبلغ المدخل أكبر من المتبقي ({purchase.RemainingAmount:N2} دج)");
                }

                var payment = new SupplierPayment
                {
                    SupplierId = model.SupplierId,
                    PurchaseId = model.PurchaseId,
                    Amount = model.Amount,
                    PaymentDate = model.PaymentDate,
                    Notes = model.Notes,
                    CreatedAt = DateTime.Now
                };

                _context.SupplierPayments.Add(payment);

                // تحديث المبالغ في الفاتورة (إن وُجدت)
                if (purchase != null)
                {
                    purchase.PaidAmount += model.Amount;
                    purchase.RemainingAmount -= model.Amount;
                    purchase.UpdatedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                await _activityLogService.LogCreateAsync(
                    module: "Purchases",
                    entityName: "SupplierPayment",
                    entityId: payment.Id,
                    description: $"تسجيل دفعة بمبلغ {model.Amount:N2} دج للمورد {supplier.Name}" +
                                 (purchase != null ? $" على الفاتورة {purchase.InvoiceNumber}" : ""));

                await transaction.CommitAsync();
                return (true, "تم تسجيل الدفعة بنجاح");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════
        // 🔍 الاستعلامات
        // ═══════════════════════════════════════════════════

        public async Task<PurchaseListViewModel> GetPurchasesAsync(
            string? search = null,
            int? supplierId = null,
            InvoiceStatus? status = null,
            PaymentType? paymentType = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                .AsQueryable();

            // تطبيق الفلاتر
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.InvoiceNumber.Contains(search) ||
                    (p.Supplier != null && p.Supplier.Name.Contains(search)) ||
                    (p.SupplierInvoiceNumber != null && p.SupplierInvoiceNumber.Contains(search)));
            }

            if (supplierId.HasValue)
                query = query.Where(p => p.SupplierId == supplierId.Value);

            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            if (paymentType.HasValue)
                query = query.Where(p => p.PaymentType == paymentType.Value);

            if (dateFrom.HasValue)
                query = query.Where(p => p.PurchaseDate >= dateFrom.Value);

            if (dateTo.HasValue)
            {
                var endDate = dateTo.Value.Date.AddDays(1);
                query = query.Where(p => p.PurchaseDate < endDate);
            }

            // العدد الإجمالي
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Pagination
            var purchases = await query
                .OrderByDescending(p => p.PurchaseDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // الإحصائيات
            var stats = await GetStatsAsync();

            // الموردين للفلتر
            var suppliers = await _context.Suppliers
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return new PurchaseListViewModel
            {
                Purchases = purchases,
                Suppliers = suppliers,
                Stats = stats,
                SearchTerm = search,
                SupplierId = supplierId,
                Status = status,
                PaymentType = paymentType,
                DateFrom = dateFrom,
                DateTo = dateTo,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            };
        }

        public async Task<PurchaseDetailsViewModel?> GetPurchaseDetailsAsync(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(i => i.Product)
                .Include(p => p.Returns)
                    .ThenInclude(r => r.ReturnItems)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null) return null;

            var payments = await _context.SupplierPayments
                .Where(sp => sp.PurchaseId == id)
                .OrderByDescending(sp => sp.PaymentDate)
                .ToListAsync();

            var activityLogs = await _activityLogService.GetEntityHistoryAsync(
                "Purchase", id, 20);

            // ✅ جلب إعدادات الشركة (للطباعة)
            var companySettings = await _context.CompanySettings.FirstOrDefaultAsync();

            return new PurchaseDetailsViewModel
            {
                Purchase = purchase,
                Returns = purchase.Returns.ToList(),
                Payments = payments,
                ActivityLogs = activityLogs,
                CompanySettings = companySettings  // ✅ جديد
            };
        }

        public async Task<PurchaseCreateViewModel?> GetPurchaseForEditAsync(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.PurchaseItems)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null) return null;
            if (!purchase.Status.CanEdit()) return null;

            return new PurchaseCreateViewModel
            {
                Id = purchase.Id,
                InvoiceNumber = purchase.InvoiceNumber,
                SupplierInvoiceNumber = purchase.SupplierInvoiceNumber,
                SupplierId = purchase.SupplierId,
                PurchaseDate = purchase.PurchaseDate,
                PaymentType = purchase.PaymentType,
                PaidAmount = purchase.PaidAmount,
                DiscountPercentage = purchase.DiscountPercentage,
                Discount = purchase.Discount,
                TransporterName = purchase.TransporterName,
                TransporterPhone = purchase.TransporterPhone,
                DeliveryNoteNumber = purchase.DeliveryNoteNumber,
                ShippingCost = purchase.ShippingCost,
                ShippingDate = purchase.ShippingDate,
                ShippingStatus = purchase.ShippingStatus,
                Notes = purchase.Notes,
                SaveAsDraft = purchase.Status == InvoiceStatus.Draft,
                Items = purchase.PurchaseItems.Select(i => new PurchaseItemCreateViewModel
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Discount = i.Discount,
                    TaxRate = i.TaxRate,
                    Notes = i.Notes,
                    BatchNumber = i.BatchNumber,
                    BatchExpiryDate = i.BatchExpiryDate
                }).ToList()
            };
        }

        public async Task<PurchaseReceiveViewModel?> GetPurchaseForReceiveAsync(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.PurchaseItems)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null) return null;
            if (!purchase.Status.CanReceive()) return null;

            return new PurchaseReceiveViewModel
            {
                PurchaseId = purchase.Id,
                InvoiceNumber = purchase.InvoiceNumber,
                ReceivedDate = DateTime.Now,
                ShippingStatus = ShippingStatus.Delivered,
                Items = purchase.PurchaseItems.Select(i => new PurchaseReceiveItemViewModel
                {
                    PurchaseItemId = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "",
                    OrderedQuantity = i.Quantity,
                    ReceivedQuantity = i.Quantity  // افتراضياً = المطلوب
                }).ToList()
            };
        }

        // ═══════════════════════════════════════════════════
        // 📊 الإحصائيات
        // ═══════════════════════════════════════════════════

        public async Task<PurchaseStatsViewModel> GetStatsAsync()
        {
            var purchases = await _context.Purchases.ToListAsync();
            var returnsCount = await _context.PurchaseReturns
                .Where(r => !r.IsCancelled)
                .CountAsync();
            var totalReturns = await _context.PurchaseReturns
                .Where(r => !r.IsCancelled)
                .SumAsync(r => r.TotalAmount);

            return new PurchaseStatsViewModel
            {
                TotalAmount = purchases.Sum(p => p.TotalAmount),
                TotalPaid = purchases.Sum(p => p.PaidAmount),
                TotalDebt = purchases.Sum(p => p.RemainingAmount),
                TotalShipping = purchases.Sum(p => p.ShippingCost),
                TotalReturns = totalReturns,
                TotalCount = purchases.Count,
                DraftCount = purchases.Count(p => p.Status == InvoiceStatus.Draft),
                SentCount = purchases.Count(p => p.Status == InvoiceStatus.Sent),
                ReceivedCount = purchases.Count(p => p.Status == InvoiceStatus.Received),
                CancelledCount = purchases.Count(p => p.Status == InvoiceStatus.Cancelled),
                UnpaidCount = purchases.Count(p => p.RemainingAmount > 0 && p.Status != InvoiceStatus.Cancelled),
                ReturnsCount = returnsCount
            };
        }

        // ═══════════════════════════════════════════════════
        // 🛠️ Helpers
        // ═══════════════════════════════════════════════════

        public async Task<string> GenerateInvoiceNumberAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"PUR-{year}-";

            var lastNumber = await _context.Purchases
                .Where(p => p.InvoiceNumber.StartsWith(prefix))
                .OrderByDescending(p => p.InvoiceNumber)
                .Select(p => p.InvoiceNumber)
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

        public async Task<bool> CanEditAsync(int id)
        {
            var purchase = await _context.Purchases.FindAsync(id);
            return purchase != null && purchase.Status.CanEdit();
        }

        public async Task<bool> CanDeleteAsync(int id)
        {
            var purchase = await _context.Purchases.FindAsync(id);
            return purchase != null && purchase.Status.CanDelete();
        }

        public async Task<bool> CanReceiveAsync(int id)
        {
            var purchase = await _context.Purchases.FindAsync(id);
            return purchase != null && purchase.Status.CanReceive();
        }

        public async Task<bool> CanCancelAsync(int id)
        {
            var purchase = await _context.Purchases.FindAsync(id);
            return purchase != null && purchase.Status.CanCancel();
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

        private void CalculatePayment(Purchase purchase, PaymentType paymentType, decimal paidAmount)
        {
            var total = purchase.TotalAmount + purchase.ShippingCost;

            switch (paymentType)
            {
                case PaymentType.Cash:
                    purchase.PaidAmount = total;
                    purchase.RemainingAmount = 0;
                    break;

                case PaymentType.Credit:
                    purchase.PaidAmount = 0;
                    purchase.RemainingAmount = total;
                    break;

                case PaymentType.Partial:
                    purchase.PaidAmount = Math.Min(paidAmount, total);
                    purchase.RemainingAmount = total - purchase.PaidAmount;
                    break;
            }
        }
    }
}