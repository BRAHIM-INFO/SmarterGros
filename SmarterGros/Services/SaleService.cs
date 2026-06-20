using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmarterGros.Data;
using SmarterGros.Models;
using SmarterGros.Models.Enums;
using SmarterGros.ViewModels;

namespace SmarterGros.Services
{
    /// <summary>
    /// 💰 خدمة المبيعات - المنطق الكامل
    /// </summary>
    public class SaleService : ISaleService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IActivityLogService _activityLogService;
        private readonly ICashRegisterService _cashRegisterService;

        public SaleService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            IActivityLogService activityLogService,
            ICashRegisterService cashRegisterService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _activityLogService = activityLogService;
            _cashRegisterService = cashRegisterService;
        }

        // ═══════════════════════════════════════════════════
        // 🆕 إنشاء فاتورة بيع
        // ═══════════════════════════════════════════════════

        public async Task<(bool Success, string Message, int? SaleId)> CreateSaleAsync(
            SaleCreateViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // التحقق من البيانات
                if (model.Items == null || !model.Items.Any())
                    return (false, "يجب إضافة منتج واحد على الأقل", null);

                // التحقق من العميل (إن وُجد)
                Customer? customer = null;
                if (model.CustomerId.HasValue)
                {
                    customer = await _context.Customers.FindAsync(model.CustomerId.Value);
                    if (customer == null)
                        return (false, "العميل غير موجود", null);
                }

                // التحقق من المخزون لكل منتج
                foreach (var item in model.Items)
                {
                    var (available, message) = await CheckStockAvailabilityAsync(
                        item.ProductId, item.Quantity);
                    if (!available)
                        return (false, message, null);
                }

                var currentUser = await GetCurrentUserAsync();
                var invoiceNumber = await GenerateInvoiceNumberAsync();

                // إنشاء الفاتورة
                var sale = new Sale
                {
                    InvoiceNumber = invoiceNumber,
                    SaleDate = model.SaleDate,
                    CustomerId = model.CustomerId,
                    CustomerName = customer?.Name ?? model.CustomerName ?? "عميل نقدي",
                    PriceType = model.PriceType,
                    PaymentType = model.PaymentType,
                    DiscountPercentage = model.DiscountPercentage,
                    Notes = model.Notes,
                    Status = model.SaveAsDraft ? InvoiceStatus.Draft : InvoiceStatus.Received,
                    CreatedById = currentUser?.Id,
                    CreatedByName = currentUser?.FullName ?? currentUser?.UserName,
                    CreatedAt = DateTime.Now
                };

                // إضافة البنود وحساب المجاميع
                decimal subTotal = 0;
                decimal totalTax = 0;
                decimal totalCost = 0;

                foreach (var itemModel in model.Items)
                {
                    var product = await _context.Products.FindAsync(itemModel.ProductId);
                    if (product == null)
                        return (false, $"المنتج رقم {itemModel.ProductId} غير موجود", null);

                    // حسابات البند
                    var itemSubTotal = itemModel.Quantity * itemModel.UnitPrice;
                    var itemDiscount = itemSubTotal * (itemModel.Discount / 100);
                    var itemAfterDiscount = itemSubTotal - itemDiscount;
                    var itemTax = itemAfterDiscount * (itemModel.TaxRate / 100);
                    var itemTotal = itemAfterDiscount + itemTax;

                    // حساب التكلفة والربح
                    var itemUnitCost = itemModel.UnitCost > 0
                        ? itemModel.UnitCost
                        : product.PurchasePriceTTC;
                    var itemTotalCost = itemModel.Quantity * itemUnitCost;
                    var itemProfit = itemAfterDiscount - itemTotalCost;

                    var saleItem = new SaleItem
                    {
                        ProductId = itemModel.ProductId,
                        Quantity = itemModel.Quantity,
                        UnitPrice = itemModel.UnitPrice,
                        UnitCost = itemUnitCost,
                        Discount = itemModel.Discount,
                        TaxRate = itemModel.TaxRate,
                        TotalPrice = itemTotal,
                        Profit = itemProfit,
                        Notes = itemModel.Notes
                    };

                    sale.SaleItems.Add(saleItem);

                    subTotal += itemAfterDiscount;
                    totalTax += itemTax;
                    totalCost += itemTotalCost;
                }

                // حساب المجاميع
                sale.SubTotal = subTotal;
                sale.TaxAmount = totalTax;
                sale.Discount = model.Discount;
                sale.TotalAmount = subTotal + totalTax - model.Discount;
                sale.TotalCost = totalCost;
                sale.TotalProfit = sale.TotalAmount - totalCost;

                // حساب الدفع
                CalculatePayment(sale, model.PaymentType, model.PaidAmount);

                // ─── حفظ الفاتورة أولاً ───
                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();

                // ─── إذا الفاتورة مكتملة (وليست مسودة) ───
                if (sale.Status == InvoiceStatus.Received)
                {
                    // تأثير المخزون (نقص!)
                    foreach (var item in sale.SaleItems)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product == null) continue;

                        var oldStock = product.StockQuantity;
                        product.StockQuantity -= item.Quantity;

                        // تسجيل حركة المخزون
                        _context.StockMovements.Add(new StockMovement
                        {
                            ProductId = item.ProductId,
                            MovementType = "إخراج - بيع",
                            Quantity = -item.Quantity,
                            QuantityBefore = oldStock,
                            QuantityAfter = product.StockQuantity,
                            Reason = $"فاتورة بيع {sale.InvoiceNumber}",
                            UserId = currentUser?.Id,
                            UserName = currentUser?.FullName ?? currentUser?.UserName,
                            MovementDate = sale.SaleDate
                        });
                    }

                    // تسجيل الدفعة (إذا Cash أو Partial)
                    if (sale.PaidAmount > 0 && sale.CustomerId.HasValue)
                    {
                        var payment = new CustomerPayment
                        {
                            CustomerId = sale.CustomerId.Value,
                            SaleId = sale.Id,
                            Amount = sale.PaidAmount,
                            PaymentDate = sale.SaleDate,
                            Notes = $"دفعة عند إنشاء الفاتورة {sale.InvoiceNumber}",
                            CreatedAt = DateTime.Now
                        };
                        _context.CustomerPayments.Add(payment);
                    }

                    await _context.SaveChangesAsync();

                    // ✅ التكامل مع الصندوق - إضافة المبلغ المدفوع
                    if (sale.PaidAmount > 0)
                    {
                        // استخدام AddTransactionAsync لإضافة وارد
                        var cashTransaction = new CashTransactionViewModel
                        {
                            CashRegisterId = (await _cashRegisterService.GetDefaultRegisterAsync())?.Id ?? 0,
                            TransactionDate = sale.SaleDate,
                            Type = TransactionType.Income,
                            Category = TransactionCategory.Sale,
                            PaymentMethod = PaymentMethod.Cash,
                            Amount = sale.PaidAmount,
                            Description = $"مبيعات نقدية - فاتورة {sale.InvoiceNumber}",
                            CustomerId = sale.CustomerId,
                            ReferenceType = "Sale",
                            ReferenceId = sale.Id,
                            ReferenceNumber = sale.InvoiceNumber
                        };

                        if (cashTransaction.CashRegisterId > 0)
                        {
                            await _cashRegisterService.AddTransactionAsync(cashTransaction);
                        }
                    }
                }

                // ─── Activity Log ───
                await _activityLogService.LogCreateAsync(
                    module: "Sales",
                    entityName: "Sale",
                    entityId: sale.Id,
                    description: $"إنشاء فاتورة بيع {sale.InvoiceNumber} - " +
                                 $"العميل: {sale.CustomerName} - " +
                                 $"المبلغ: {sale.TotalAmount:N2} دج - " +
                                 $"الربح: {sale.TotalProfit:N2} دج",
                    newValues: new
                    {
                        sale.InvoiceNumber,
                        CustomerName = sale.CustomerName,
                        sale.TotalAmount,
                        sale.TotalProfit,
                        sale.PaymentType,
                        sale.Status
                    });

                await transaction.CommitAsync();
                return (true,
                    $"تم إنشاء الفاتورة {invoiceNumber} بنجاح - " +
                    $"الربح: {sale.TotalProfit:N2} دج",
                    sale.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _activityLogService.LogErrorAsync(
                    actionName: "إنشاء فاتورة بيع",
                    errorMessage: ex.Message,
                    module: "Sales");
                return (false, $"حدث خطأ: {ex.Message}", null);
            }
        }

        // ═══════════════════════════════════════════════════
        // ✏️ تعديل فاتورة (مسودة فقط)
        // ═══════════════════════════════════════════════════

        public async Task<(bool Success, string Message)> UpdateSaleAsync(
            int id, SaleCreateViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var sale = await _context.Sales
                    .Include(s => s.SaleItems)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (sale == null)
                    return (false, "الفاتورة غير موجودة");

                if (sale.Status != InvoiceStatus.Draft)
                    return (false, "لا يمكن تعديل هذه الفاتورة - فقط المسودات قابلة للتعديل");

                // حذف البنود القديمة
                _context.SaleItems.RemoveRange(sale.SaleItems);

                // تحديث البيانات
                sale.CustomerId = model.CustomerId;
                sale.SaleDate = model.SaleDate;
                sale.PriceType = model.PriceType;
                sale.PaymentType = model.PaymentType;
                sale.DiscountPercentage = model.DiscountPercentage;
                sale.Notes = model.Notes;
                sale.UpdatedAt = DateTime.Now;
                sale.SaleItems = new List<SaleItem>();

                // إضافة البنود الجديدة
                decimal subTotal = 0;
                decimal totalTax = 0;
                decimal totalCost = 0;

                foreach (var itemModel in model.Items)
                {
                    var product = await _context.Products.FindAsync(itemModel.ProductId);
                    if (product == null) continue;

                    var itemSubTotal = itemModel.Quantity * itemModel.UnitPrice;
                    var itemDiscount = itemSubTotal * (itemModel.Discount / 100);
                    var itemAfterDiscount = itemSubTotal - itemDiscount;
                    var itemTax = itemAfterDiscount * (itemModel.TaxRate / 100);
                    var itemTotal = itemAfterDiscount + itemTax;

                    var itemUnitCost = itemModel.UnitCost > 0
                        ? itemModel.UnitCost
                        : product.PurchasePriceTTC;
                    var itemTotalCost = itemModel.Quantity * itemUnitCost;
                    var itemProfit = itemAfterDiscount - itemTotalCost;

                    sale.SaleItems.Add(new SaleItem
                    {
                        SaleId = sale.Id,
                        ProductId = itemModel.ProductId,
                        Quantity = itemModel.Quantity,
                        UnitPrice = itemModel.UnitPrice,
                        UnitCost = itemUnitCost,
                        Discount = itemModel.Discount,
                        TaxRate = itemModel.TaxRate,
                        TotalPrice = itemTotal,
                        Profit = itemProfit,
                        Notes = itemModel.Notes
                    });

                    subTotal += itemAfterDiscount;
                    totalTax += itemTax;
                    totalCost += itemTotalCost;
                }

                sale.SubTotal = subTotal;
                sale.TaxAmount = totalTax;
                sale.Discount = model.Discount;
                sale.TotalAmount = subTotal + totalTax - model.Discount;
                sale.TotalCost = totalCost;
                sale.TotalProfit = sale.TotalAmount - totalCost;

                CalculatePayment(sale, model.PaymentType, model.PaidAmount);

                await _context.SaveChangesAsync();

                await _activityLogService.LogUpdateAsync(
                    module: "Sales",
                    entityName: "Sale",
                    entityId: sale.Id,
                    description: $"تعديل الفاتورة {sale.InvoiceNumber}");

                await transaction.CommitAsync();
                return (true, "تم تعديل الفاتورة بنجاح");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════
        // 🗑️ حذف فاتورة (مسودة فقط)
        // ═══════════════════════════════════════════════════

        public async Task<(bool Success, string Message)> DeleteSaleAsync(int id)
        {
            try
            {
                var sale = await _context.Sales
                    .Include(s => s.SaleItems)
                    .Include(s => s.Customer)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (sale == null)
                    return (false, "الفاتورة غير موجودة");

                if (sale.Status != InvoiceStatus.Draft)
                    return (false, "لا يمكن حذف هذه الفاتورة - فقط المسودات");

                _context.SaleItems.RemoveRange(sale.SaleItems);
                _context.Sales.Remove(sale);
                await _context.SaveChangesAsync();

                await _activityLogService.LogDeleteAsync(
                    module: "Sales",
                    entityName: "Sale",
                    entityId: id,
                    description: $"حذف الفاتورة {sale.InvoiceNumber}");

                return (true, "تم حذف الفاتورة بنجاح");
            }
            catch (Exception ex)
            {
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════
        // ❌ إلغاء فاتورة (عكس التأثيرات)
        // ═══════════════════════════════════════════════════

        public async Task<(bool Success, string Message)> CancelSaleAsync(
            SaleCancelViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var sale = await _context.Sales
                    .Include(s => s.SaleItems)
                        .ThenInclude(i => i.Product)
                    .Include(s => s.Customer)
                    .FirstOrDefaultAsync(s => s.Id == model.SaleId);

                if (sale == null)
                    return (false, "الفاتورة غير موجودة");

                if (sale.Status == InvoiceStatus.Cancelled)
                    return (false, "الفاتورة ملغاة بالفعل");

                if (!model.ConfirmReversal)
                    return (false, "يجب تأكيد عكس التأثيرات");

                var currentUser = await GetCurrentUserAsync();
                var wasCompleted = sale.Status == InvoiceStatus.Received;

                // ─── إذا كانت مكتملة، نعكس التأثيرات ───
                if (wasCompleted)
                {
                    // عكس تأثير المخزون (إرجاع الكميات)
                    foreach (var item in sale.SaleItems)
                    {
                        if (item.Product == null) continue;

                        var qtyToReturn = item.Quantity - item.ReturnedQuantity;
                        if (qtyToReturn <= 0) continue;

                        var oldStock = item.Product.StockQuantity;
                        item.Product.StockQuantity += qtyToReturn;

                        _context.StockMovements.Add(new StockMovement
                        {
                            ProductId = item.ProductId,
                            MovementType = "إدخال - إلغاء بيع",
                            Quantity = qtyToReturn,
                            QuantityBefore = oldStock,
                            QuantityAfter = item.Product.StockQuantity,
                            Reason = $"إلغاء فاتورة بيع {sale.InvoiceNumber}",
                            UserId = currentUser?.Id,
                            UserName = currentUser?.FullName ?? currentUser?.UserName,
                            MovementDate = DateTime.Now,
                            Notes = $"السبب: {model.CancellationReason}"
                        });
                    }

                    // عكس تأثير الصندوق (إذا كان فيه مدفوع)
                    if (sale.PaidAmount > 0)
                    {
                        var register = await _cashRegisterService.GetDefaultRegisterAsync();
                        if (register != null)
                        {
                            var cashTransaction = new CashTransactionViewModel
                            {
                                CashRegisterId = register.Id,
                                TransactionDate = DateTime.Now,
                                Type = TransactionType.Expense,
                                Category = TransactionCategory.CustomerRefund,
                                PaymentMethod = PaymentMethod.Cash,
                                Amount = sale.PaidAmount,
                                Description = $"عكس مبيعات - إلغاء فاتورة {sale.InvoiceNumber}",
                                CustomerId = sale.CustomerId,
                                ReferenceType = "SaleCancellation",
                                ReferenceId = sale.Id,
                                ReferenceNumber = sale.InvoiceNumber
                            };

                            await _cashRegisterService.AddTransactionAsync(cashTransaction);
                        }
                    }
                }

                // تحديث حالة الفاتورة
                sale.Status = InvoiceStatus.Cancelled;
                sale.CancellationReason = model.CancellationReason;
                sale.CancelledAt = DateTime.Now;
                sale.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                await _activityLogService.LogAsync(
                    actionType: "Cancel",
                    actionName: "إلغاء فاتورة بيع",
                    module: "Sales",
                    entityName: "Sale",
                    entityId: sale.Id,
                    description: $"إلغاء الفاتورة {sale.InvoiceNumber} - السبب: {model.CancellationReason}",
                    severity: "Critical");

                await transaction.CommitAsync();
                return (true, $"تم إلغاء الفاتورة {sale.InvoiceNumber} بنجاح" +
                    (wasCompleted ? " وتم عكس التأثيرات على المخزون والصندوق" : ""));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════
        // 📋 نسخ فاتورة
        // ═══════════════════════════════════════════════════

        public async Task<(bool Success, string Message, int? NewSaleId)> DuplicateSaleAsync(
            int sourceId)
        {
            try
            {
                var source = await _context.Sales
                    .Include(s => s.SaleItems)
                    .FirstOrDefaultAsync(s => s.Id == sourceId);

                if (source == null)
                    return (false, "الفاتورة المصدر غير موجودة", null);

                var currentUser = await GetCurrentUserAsync();
                var invoiceNumber = await GenerateInvoiceNumberAsync();

                var newSale = new Sale
                {
                    InvoiceNumber = invoiceNumber,
                    CustomerId = source.CustomerId,
                    CustomerName = source.CustomerName,
                    SaleDate = DateTime.Now,
                    Status = InvoiceStatus.Draft,
                    PriceType = source.PriceType,
                    PaymentType = PaymentType.Cash,
                    SubTotal = source.SubTotal,
                    TaxAmount = source.TaxAmount,
                    Discount = source.Discount,
                    DiscountPercentage = source.DiscountPercentage,
                    TotalAmount = source.TotalAmount,
                    TotalCost = source.TotalCost,
                    TotalProfit = source.TotalProfit,
                    Notes = $"نسخة من {source.InvoiceNumber}",
                    CreatedById = currentUser?.Id,
                    CreatedByName = currentUser?.FullName ?? currentUser?.UserName,
                    CreatedAt = DateTime.Now
                };

                foreach (var sourceItem in source.SaleItems)
                {
                    newSale.SaleItems.Add(new SaleItem
                    {
                        ProductId = sourceItem.ProductId,
                        Quantity = sourceItem.Quantity,
                        UnitPrice = sourceItem.UnitPrice,
                        UnitCost = sourceItem.UnitCost,
                        Discount = sourceItem.Discount,
                        TaxRate = sourceItem.TaxRate,
                        TotalPrice = sourceItem.TotalPrice,
                        Profit = sourceItem.Profit
                    });
                }

                _context.Sales.Add(newSale);
                await _context.SaveChangesAsync();

                await _activityLogService.LogCreateAsync(
                    module: "Sales",
                    entityName: "Sale",
                    entityId: newSale.Id,
                    description: $"نسخ فاتورة: {source.InvoiceNumber} → {newSale.InvoiceNumber}");

                return (true, $"تم نسخ الفاتورة بنجاح برقم {invoiceNumber}", newSale.Id);
            }
            catch (Exception ex)
            {
                return (false, $"حدث خطأ: {ex.Message}", null);
            }
        }

        // ═══════════════════════════════════════════════════
        // 💳 تسجيل دفعة من عميل
        // ═══════════════════════════════════════════════════

        public async Task<(bool Success, string Message)> RegisterPaymentAsync(
            CustomerPaymentViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var customer = await _context.Customers.FindAsync(model.CustomerId);
                if (customer == null)
                    return (false, "العميل غير موجود");

                Sale? sale = null;
                if (model.SaleId.HasValue)
                {
                    sale = await _context.Sales.FindAsync(model.SaleId.Value);
                    if (sale == null)
                        return (false, "الفاتورة غير موجودة");

                    if (model.Amount > sale.RemainingAmount)
                        return (false, $"المبلغ المدخل أكبر من المتبقي ({sale.RemainingAmount:N2} دج)");
                }

                var payment = new CustomerPayment
                {
                    CustomerId = model.CustomerId,
                    SaleId = model.SaleId,
                    Amount = model.Amount,
                    PaymentDate = model.PaymentDate,
                    Notes = model.Notes,
                    CreatedAt = DateTime.Now
                };

                _context.CustomerPayments.Add(payment);

                // تحديث الفاتورة (إن وُجدت)
                if (sale != null)
                {
                    sale.PaidAmount += model.Amount;
                    sale.RemainingAmount -= model.Amount;
                    sale.UpdatedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                // ✅ التكامل مع الصندوق
                var register = await _cashRegisterService.GetDefaultRegisterAsync();
                if (register != null)
                {
                    var cashTransaction = new CashTransactionViewModel
                    {
                        CashRegisterId = register.Id,
                        TransactionDate = model.PaymentDate,
                        Type = TransactionType.Income,
                        Category = TransactionCategory.CustomerPayment,
                        PaymentMethod = PaymentMethod.Cash,
                        Amount = model.Amount,
                        Description = $"تحصيل من العميل {customer.Name}" +
                                     (sale != null ? $" - فاتورة {sale.InvoiceNumber}" : ""),
                        CustomerId = model.CustomerId,
                        ReferenceType = "CustomerPayment",
                        ReferenceId = payment.Id
                    };

                    await _cashRegisterService.AddTransactionAsync(cashTransaction);
                }

                await _activityLogService.LogCreateAsync(
                    module: "Sales",
                    entityName: "CustomerPayment",
                    entityId: payment.Id,
                    description: $"تحصيل {model.Amount:N2} دج من العميل {customer.Name}");

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

        public async Task<SaleListViewModel> GetSalesAsync(
            string? search = null,
            int? customerId = null,
            InvoiceStatus? status = null,
            PaymentType? paymentType = null,
            SalePriceType? priceType = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                .AsQueryable();

            // الفلاتر
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s =>
                    s.InvoiceNumber.Contains(search) ||
                    (s.Customer != null && s.Customer.Name.Contains(search)) ||
                    (s.CustomerName != null && s.CustomerName.Contains(search)));
            }

            if (customerId.HasValue)
                query = query.Where(s => s.CustomerId == customerId.Value);

            if (status.HasValue)
                query = query.Where(s => s.Status == status.Value);

            if (paymentType.HasValue)
                query = query.Where(s => s.PaymentType == paymentType.Value);

            if (priceType.HasValue)
                query = query.Where(s => s.PriceType == priceType.Value);

            if (dateFrom.HasValue)
                query = query.Where(s => s.SaleDate >= dateFrom.Value);

            if (dateTo.HasValue)
            {
                var endDate = dateTo.Value.Date.AddDays(1);
                query = query.Where(s => s.SaleDate < endDate);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var sales = await query
                .OrderByDescending(s => s.SaleDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var stats = await GetStatsAsync();

            var customers = await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return new SaleListViewModel
            {
                Sales = sales,
                Customers = customers,
                Stats = stats,
                SearchTerm = search,
                CustomerId = customerId,
                Status = status,
                PaymentType = paymentType,
                PriceType = priceType,
                DateFrom = dateFrom,
                DateTo = dateTo,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            };
        }

        public async Task<SaleDetailsViewModel?> GetSaleDetailsAsync(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null) return null;

            var payments = await _context.CustomerPayments
                .Where(cp => cp.SaleId == id)
                .OrderByDescending(cp => cp.PaymentDate)
                .ToListAsync();

            var activityLogs = await _activityLogService.GetEntityHistoryAsync(
                "Sale", id, 20);

            var companySettings = await _context.CompanySettings.FirstOrDefaultAsync();

            return new SaleDetailsViewModel
            {
                Sale = sale,
                Payments = payments,
                ActivityLogs = activityLogs,
                CompanySettings = companySettings
            };
        }

        public async Task<SaleCreateViewModel?> GetSaleForEditAsync(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.SaleItems)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null) return null;
            if (sale.Status != InvoiceStatus.Draft) return null;

            return new SaleCreateViewModel
            {
                Id = sale.Id,
                InvoiceNumber = sale.InvoiceNumber,
                CustomerId = sale.CustomerId,
                CustomerName = sale.CustomerName,
                SaleDate = sale.SaleDate,
                PriceType = sale.PriceType,
                PaymentType = sale.PaymentType,
                PaidAmount = sale.PaidAmount,
                DiscountPercentage = sale.DiscountPercentage,
                Discount = sale.Discount,
                Notes = sale.Notes,
                SaveAsDraft = true,
                Items = sale.SaleItems.Select(i => new SaleItemCreateViewModel
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    UnitCost = i.UnitCost,
                    Discount = i.Discount,
                    TaxRate = i.TaxRate,
                    Notes = i.Notes
                }).ToList()
            };
        }

        // ═══════════════════════════════════════════════════
        // 📊 الإحصائيات
        // ═══════════════════════════════════════════════════

        public async Task<SaleStatsViewModel> GetStatsAsync()
        {
            var sales = await _context.Sales.ToListAsync();
            var activeSales = sales.Where(s => s.Status != InvoiceStatus.Cancelled).ToList();

            return new SaleStatsViewModel
            {
                TotalAmount = activeSales.Sum(s => s.TotalAmount),
                TotalCollected = activeSales.Sum(s => s.PaidAmount),
                TotalDebt = activeSales.Sum(s => s.RemainingAmount),
                TotalCost = activeSales.Sum(s => s.TotalCost),
                TotalProfit = activeSales.Sum(s => s.TotalProfit),
                TotalCount = sales.Count,
                CompletedCount = sales.Count(s => s.Status == InvoiceStatus.Received),
                CancelledCount = sales.Count(s => s.Status == InvoiceStatus.Cancelled),
                UnpaidCount = activeSales.Count(s => s.RemainingAmount > 0),
                CashSalesCount = activeSales.Count(s => s.PaymentType == PaymentType.Cash),
                CreditSalesCount = activeSales.Count(s => s.PaymentType == PaymentType.Credit),
                PartialSalesCount = activeSales.Count(s => s.PaymentType == PaymentType.Partial),
                CashSalesAmount = activeSales
                    .Where(s => s.PaymentType == PaymentType.Cash)
                    .Sum(s => s.TotalAmount),
                CreditSalesAmount = activeSales
                    .Where(s => s.PaymentType == PaymentType.Credit)
                    .Sum(s => s.TotalAmount)
            };
        }

        // ═══════════════════════════════════════════════════
        // 🛠️ Helpers
        // ═══════════════════════════════════════════════════

        public async Task<string> GenerateInvoiceNumberAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"SAL-{year}-";

            var lastNumber = await _context.Sales
                .Where(s => s.InvoiceNumber.StartsWith(prefix))
                .OrderByDescending(s => s.InvoiceNumber)
                .Select(s => s.InvoiceNumber)
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

        public async Task<(bool Available, string Message)> CheckStockAvailabilityAsync(
            int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return (false, "المنتج غير موجود");

            if (product.StockQuantity < quantity)
                return (false, $"المخزون غير كافٍ للمنتج '{product.Name}'. " +
                              $"المتوفر: {product.StockQuantity}، المطلوب: {quantity}");

            return (true, "");
        }

        public async Task<bool> CanEditAsync(int id)
        {
            var sale = await _context.Sales.FindAsync(id);
            return sale != null && sale.Status == InvoiceStatus.Draft;
        }

        public async Task<bool> CanDeleteAsync(int id)
        {
            var sale = await _context.Sales.FindAsync(id);
            return sale != null && sale.Status == InvoiceStatus.Draft;
        }

        public async Task<bool> CanCancelAsync(int id)
        {
            var sale = await _context.Sales.FindAsync(id);
            return sale != null && sale.Status != InvoiceStatus.Cancelled;
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

        private void CalculatePayment(Sale sale, PaymentType paymentType, decimal paidAmount)
        {
            switch (paymentType)
            {
                case PaymentType.Cash:
                    sale.PaidAmount = sale.TotalAmount;
                    sale.RemainingAmount = 0;
                    break;

                case PaymentType.Credit:
                    sale.PaidAmount = 0;
                    sale.RemainingAmount = sale.TotalAmount;
                    break;

                case PaymentType.Partial:
                    sale.PaidAmount = Math.Min(paidAmount, sale.TotalAmount);
                    sale.RemainingAmount = sale.TotalAmount - sale.PaidAmount;
                    break;
            }
        }
    }
}