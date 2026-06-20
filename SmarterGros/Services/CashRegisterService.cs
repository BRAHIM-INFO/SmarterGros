using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmarterGros.Data;
using SmarterGros.Models;
using SmarterGros.Models.Enums;
using SmarterGros.ViewModels;

namespace SmarterGros.Services
{
    /// <summary>
    /// 💰 خدمة الصندوق - المنطق الكامل
    /// </summary>
    public class CashRegisterService : ICashRegisterService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IActivityLogService _activityLogService;

        public CashRegisterService(
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
        // 📊 لوحة المعلومات
        // ═══════════════════════════════════════════════════

        public async Task<CashDashboardViewModel> GetDashboardAsync(int? cashRegisterId = null)
        {
            var register = cashRegisterId.HasValue
                ? await _context.CashRegisters.FindAsync(cashRegisterId.Value)
                : await GetDefaultRegisterAsync();

            if (register == null)
            {
                return new CashDashboardViewModel();
            }

            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var yearStart = new DateTime(today.Year, 1, 1);

            // الحركات النشطة فقط
            var activeTransactions = _context.CashTransactions
                .Where(t => t.CashRegisterId == register.Id && !t.IsCancelled);

            // إحصائيات اليوم
            var todayTransactions = await activeTransactions
                .Where(t => t.TransactionDate.Date == today)
                .ToListAsync();

            // إحصائيات الأسبوع
            var weekTransactions = await activeTransactions
                .Where(t => t.TransactionDate >= weekStart)
                .ToListAsync();

            // إحصائيات الشهر
            var monthTransactions = await activeTransactions
                .Where(t => t.TransactionDate >= monthStart)
                .ToListAsync();

            // إحصائيات السنة
            var yearTransactions = await activeTransactions
                .Where(t => t.TransactionDate >= yearStart)
                .ToListAsync();

            // آخر 10 حركات
            var recentTransactions = await _context.CashTransactions
                .Include(t => t.Supplier)
                .Include(t => t.Customer)
                .Where(t => t.CashRegisterId == register.Id)
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .ToListAsync();

            // الجرد الأخير
            var lastClosure = await _context.DailyClosures
                .Where(c => c.CashRegisterId == register.Id)
                .OrderByDescending(c => c.ClosureDate)
                .FirstOrDefaultAsync();

            // هل اليوم مغلق؟
            var isTodayClosed = await IsDayClosedAsync(register.Id, today);

            // أعلى فئات الصرف هذا الشهر
            var topExpenseCategories = monthTransactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category)
                .Select(g => new CategoryStatsViewModel
                {
                    Category = g.Key,
                    TotalAmount = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(c => c.TotalAmount)
                .Take(5)
                .ToList();

            // حساب النسب
            var totalMonthExpense = monthTransactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            foreach (var cat in topExpenseCategories)
            {
                cat.Percentage = totalMonthExpense > 0
                    ? Math.Round((cat.TotalAmount / totalMonthExpense) * 100, 2)
                    : 0;
            }

            // أعلى فئات الدخل هذا الشهر
            var topIncomeCategories = monthTransactions
                .Where(t => t.Type == TransactionType.Income)
                .GroupBy(t => t.Category)
                .Select(g => new CategoryStatsViewModel
                {
                    Category = g.Key,
                    TotalAmount = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(c => c.TotalAmount)
                .Take(5)
                .ToList();

            var totalMonthIncome = monthTransactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            foreach (var cat in topIncomeCategories)
            {
                cat.Percentage = totalMonthIncome > 0
                    ? Math.Round((cat.TotalAmount / totalMonthIncome) * 100, 2)
                    : 0;
            }

            return new CashDashboardViewModel
            {
                CurrentRegister = register,
                CurrentBalance = register.CurrentBalance,

                // اليوم
                TodayIncome = todayTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                TodayExpense = todayTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
                TodayTransactionsCount = todayTransactions.Count,

                // الأسبوع
                WeekIncome = weekTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                WeekExpense = weekTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
                WeekTransactionsCount = weekTransactions.Count,

                // الشهر
                MonthIncome = totalMonthIncome,
                MonthExpense = totalMonthExpense,
                MonthTransactionsCount = monthTransactions.Count,

                // السنة
                YearIncome = yearTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                YearExpense = yearTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
                YearTransactionsCount = yearTransactions.Count,

                RecentTransactions = recentTransactions,
                IsTodayClosed = isTodayClosed,
                LastClosure = lastClosure,
                TopExpenseCategories = topExpenseCategories,
                TopIncomeCategories = topIncomeCategories
            };
        }

        public async Task<CashRegister?> GetDefaultRegisterAsync()
        {
            return await _context.CashRegisters
                .FirstOrDefaultAsync(c => c.IsDefault && c.IsActive);
        }

        public async Task<decimal> GetCurrentBalanceAsync(int? cashRegisterId = null)
        {
            var register = cashRegisterId.HasValue
                ? await _context.CashRegisters.FindAsync(cashRegisterId.Value)
                : await GetDefaultRegisterAsync();

            return register?.CurrentBalance ?? 0;
        }

        // ═══════════════════════════════════════════════════
        // 💰 إدارة الحركات اليدوية
        // ═══════════════════════════════════════════════════

        public async Task<(bool Success, string Message, int? TransactionId)> AddTransactionAsync(
            CashTransactionViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // التحقق من الصندوق
                var register = await _context.CashRegisters.FindAsync(model.CashRegisterId);
                if (register == null)
                    return (false, "الصندوق غير موجود", null);

                if (!register.IsActive)
                    return (false, "الصندوق غير نشط", null);

                // التحقق من إغلاق اليوم
                if (await IsDayClosedAsync(register.Id, model.TransactionDate.Date))
                    return (false, "لا يمكن إضافة حركة - اليوم مغلق", null);

                // التحقق من المبلغ
                if (model.Amount <= 0)
                    return (false, "المبلغ يجب أن يكون أكبر من 0", null);

                // التحقق من توافق Type و Category
                var expectedType = model.Category.GetTransactionType();
                if (expectedType != model.Type)
                    return (false, $"الفئة المختارة لا تتوافق مع نوع الحركة ({model.Type.GetArabicName()})", null);

                // التحقق من وجود رصيد كاف (للصادرات)
                if (model.Type == TransactionType.Expense && model.Amount > register.CurrentBalance)
                {
                    return (false,
                        $"الرصيد غير كافٍ! الرصيد الحالي: {register.CurrentBalance:N2} دج", null);
                }

                var currentUser = await GetCurrentUserAsync();
                var transactionNumber = await GenerateTransactionNumberAsync();

                // إنشاء الحركة
                var cashTransaction = new CashTransaction
                {
                    TransactionNumber = transactionNumber,
                    TransactionDate = model.TransactionDate,
                    CashRegisterId = model.CashRegisterId,
                    Type = model.Type,
                    Category = model.Category,
                    PaymentMethod = model.PaymentMethod,
                    Amount = model.Amount,
                    BalanceBefore = register.CurrentBalance,
                    BalanceAfter = model.Type == TransactionType.Income
                        ? register.CurrentBalance + model.Amount
                        : register.CurrentBalance - model.Amount,
                    Description = model.Description,
                    Notes = model.Notes,
                    SupplierId = model.SupplierId,
                    CustomerId = model.CustomerId,
                    ReferenceType = model.ReferenceType,
                    ReferenceId = model.ReferenceId,
                    ReferenceNumber = model.ReferenceNumber,
                    CheckNumber = model.CheckNumber,
                    BankName = model.BankName,
                    CheckDueDate = model.CheckDueDate,
                    CreatedById = currentUser?.Id,
                    CreatedByName = currentUser?.FullName ?? currentUser?.UserName,
                    CreatedAt = DateTime.Now
                };

                _context.CashTransactions.Add(cashTransaction);

                // تحديث رصيد الصندوق
                register.CurrentBalance = cashTransaction.BalanceAfter;
                register.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                // تسجيل في Activity Log
                await _activityLogService.LogCreateAsync(
                    module: "CashRegister",
                    entityName: "CashTransaction",
                    entityId: cashTransaction.Id,
                    description: $"حركة {model.Type.GetArabicName()} - {model.Category.GetArabicName()} " +
                                 $"بمبلغ {model.Amount:N2} دج",
                    newValues: new
                    {
                        cashTransaction.TransactionNumber,
                        Type = model.Type.GetArabicName(),
                        Category = model.Category.GetArabicName(),
                        cashTransaction.Amount,
                        BalanceBefore = cashTransaction.BalanceBefore,
                        BalanceAfter = cashTransaction.BalanceAfter
                    });

                await transaction.CommitAsync();

                return (true,
                    $"تم تسجيل الحركة {transactionNumber} بنجاح - الرصيد الجديد: {register.CurrentBalance:N2} دج",
                    cashTransaction.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _activityLogService.LogErrorAsync(
                    actionName: "إضافة حركة صندوق",
                    errorMessage: ex.Message,
                    module: "CashRegister");
                return (false, $"حدث خطأ: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> CancelTransactionAsync(
            CancelCashTransactionViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cashTransaction = await _context.CashTransactions
                    .Include(t => t.CashRegister)
                    .FirstOrDefaultAsync(t => t.Id == model.TransactionId);

                if (cashTransaction == null)
                    return (false, "الحركة غير موجودة");

                if (cashTransaction.IsCancelled)
                    return (false, "الحركة ملغاة بالفعل");

                if (cashTransaction.CashRegister == null)
                    return (false, "الصندوق غير موجود");

                // التحقق من عدم وجود جرد مغلق لهذا اليوم
                if (await IsDayClosedAsync(cashTransaction.CashRegisterId, cashTransaction.TransactionDate.Date))
                    return (false, "لا يمكن إلغاء الحركة - يوم الحركة مغلق");

                var currentUser = await GetCurrentUserAsync();

                // عكس تأثير الحركة على الرصيد
                if (cashTransaction.Type == TransactionType.Income)
                {
                    cashTransaction.CashRegister.CurrentBalance -= cashTransaction.Amount;
                }
                else
                {
                    cashTransaction.CashRegister.CurrentBalance += cashTransaction.Amount;
                }

                cashTransaction.CashRegister.UpdatedAt = DateTime.Now;

                // تحديث الحركة
                cashTransaction.IsCancelled = true;
                cashTransaction.CancellationReason = model.CancellationReason;
                cashTransaction.CancelledAt = DateTime.Now;
                cashTransaction.CancelledById = currentUser?.Id;
                cashTransaction.CancelledByName = currentUser?.FullName ?? currentUser?.UserName;
                cashTransaction.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                // Activity Log
                await _activityLogService.LogAsync(
                    actionType: "Cancel",
                    actionName: "إلغاء حركة صندوق",
                    module: "CashRegister",
                    entityName: "CashTransaction",
                    entityId: cashTransaction.Id,
                    description: $"إلغاء حركة {cashTransaction.TransactionNumber} - السبب: {model.CancellationReason}",
                    severity: "Critical");

                await transaction.CommitAsync();

                return (true, $"تم إلغاء الحركة {cashTransaction.TransactionNumber} وعكس تأثيرها على الرصيد");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _activityLogService.LogErrorAsync(
                    actionName: "إلغاء حركة صندوق",
                    errorMessage: ex.Message,
                    module: "CashRegister");
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteTransactionAsync(int transactionId)
        {
            try
            {
                var cashTransaction = await _context.CashTransactions.FindAsync(transactionId);
                if (cashTransaction == null)
                    return (false, "الحركة غير موجودة");

                if (!cashTransaction.IsCancelled)
                    return (false, "لا يمكن حذف حركة نشطة - يجب إلغاؤها أولاً");

                _context.CashTransactions.Remove(cashTransaction);
                await _context.SaveChangesAsync();

                await _activityLogService.LogDeleteAsync(
                    module: "CashRegister",
                    entityName: "CashTransaction",
                    entityId: transactionId,
                    description: $"حذف حركة {cashTransaction.TransactionNumber}");

                return (true, "تم حذف الحركة بنجاح");
            }
            catch (Exception ex)
            {
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════
        // 🔄 الحركات التلقائية (من أنظمة أخرى)
        // ═══════════════════════════════════════════════════

        public async Task<bool> RecordPurchasePaymentAsync(
            int purchaseId,
            string invoiceNumber,
            decimal amount,
            int supplierId,
            string supplierName,
            string? notes = null)
        {
            try
            {
                var register = await GetDefaultRegisterAsync();
                if (register == null || amount <= 0) return false;

                var currentUser = await GetCurrentUserAsync();
                var transactionNumber = await GenerateTransactionNumberAsync();

                var cashTransaction = new CashTransaction
                {
                    TransactionNumber = transactionNumber,
                    TransactionDate = DateTime.Now,
                    CashRegisterId = register.Id,
                    Type = TransactionType.Expense,
                    Category = TransactionCategory.Purchase,
                    PaymentMethod = PaymentMethod.Cash,
                    Amount = amount,
                    BalanceBefore = register.CurrentBalance,
                    BalanceAfter = register.CurrentBalance - amount,
                    Description = $"دفع لفاتورة شراء {invoiceNumber} - المورد: {supplierName}",
                    Notes = notes,
                    SupplierId = supplierId,
                    ReferenceType = "Purchase",
                    ReferenceId = purchaseId,
                    ReferenceNumber = invoiceNumber,
                    CreatedById = currentUser?.Id,
                    CreatedByName = currentUser?.FullName ?? currentUser?.UserName,
                    CreatedAt = DateTime.Now
                };

                _context.CashTransactions.Add(cashTransaction);
                register.CurrentBalance -= amount;
                register.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                await _activityLogService.LogAsync(
                    actionType: "AutoCash",
                    actionName: "تسجيل تلقائي - دفع شراء",
                    module: "CashRegister",
                    entityName: "CashTransaction",
                    entityId: cashTransaction.Id,
                    description: $"خصم تلقائي {amount:N2} دج لفاتورة {invoiceNumber}");

                return true;
            }
            catch (Exception ex)
            {
                await _activityLogService.LogErrorAsync(
                    actionName: "تسجيل دفع شراء",
                    errorMessage: ex.Message,
                    module: "CashRegister");
                return false;
            }
        }

        public async Task<bool> RecordPurchaseRefundAsync(
            int returnId,
            string returnNumber,
            decimal amount,
            int supplierId,
            string supplierName,
            string? notes = null)
        {
            try
            {
                var register = await GetDefaultRegisterAsync();
                if (register == null || amount <= 0) return false;

                var currentUser = await GetCurrentUserAsync();
                var transactionNumber = await GenerateTransactionNumberAsync();

                var cashTransaction = new CashTransaction
                {
                    TransactionNumber = transactionNumber,
                    TransactionDate = DateTime.Now,
                    CashRegisterId = register.Id,
                    Type = TransactionType.Income,
                    Category = TransactionCategory.SupplierRefund,
                    PaymentMethod = PaymentMethod.Cash,
                    Amount = amount,
                    BalanceBefore = register.CurrentBalance,
                    BalanceAfter = register.CurrentBalance + amount,
                    Description = $"استرداد من مرتجع {returnNumber} - المورد: {supplierName}",
                    Notes = notes,
                    SupplierId = supplierId,
                    ReferenceType = "PurchaseReturn",
                    ReferenceId = returnId,
                    ReferenceNumber = returnNumber,
                    CreatedById = currentUser?.Id,
                    CreatedByName = currentUser?.FullName ?? currentUser?.UserName,
                    CreatedAt = DateTime.Now
                };

                _context.CashTransactions.Add(cashTransaction);
                register.CurrentBalance += amount;
                register.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                await _activityLogService.LogAsync(
                    actionType: "AutoCash",
                    actionName: "تسجيل تلقائي - استرداد مرتجع",
                    module: "CashRegister",
                    entityName: "CashTransaction",
                    entityId: cashTransaction.Id,
                    description: $"إضافة تلقائية {amount:N2} دج من مرتجع {returnNumber}");

                return true;
            }
            catch (Exception ex)
            {
                await _activityLogService.LogErrorAsync(
                    actionName: "تسجيل استرداد مرتجع",
                    errorMessage: ex.Message,
                    module: "CashRegister");
                return false;
            }
        }

        public async Task<bool> RecordSupplierPaymentAsync(
            int paymentId,
            decimal amount,
            int supplierId,
            string supplierName,
            string? notes = null)
        {
            try
            {
                var register = await GetDefaultRegisterAsync();
                if (register == null || amount <= 0) return false;

                var currentUser = await GetCurrentUserAsync();
                var transactionNumber = await GenerateTransactionNumberAsync();

                var cashTransaction = new CashTransaction
                {
                    TransactionNumber = transactionNumber,
                    TransactionDate = DateTime.Now,
                    CashRegisterId = register.Id,
                    Type = TransactionType.Expense,
                    Category = TransactionCategory.SupplierPayment,
                    PaymentMethod = PaymentMethod.Cash,
                    Amount = amount,
                    BalanceBefore = register.CurrentBalance,
                    BalanceAfter = register.CurrentBalance - amount,
                    Description = $"دفعة للمورد: {supplierName}",
                    Notes = notes,
                    SupplierId = supplierId,
                    ReferenceType = "SupplierPayment",
                    ReferenceId = paymentId,
                    CreatedById = currentUser?.Id,
                    CreatedByName = currentUser?.FullName ?? currentUser?.UserName,
                    CreatedAt = DateTime.Now
                };

                _context.CashTransactions.Add(cashTransaction);
                register.CurrentBalance -= amount;
                register.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ═══════════════════════════════════════════════════
        // 🔍 الاستعلامات
        // ═══════════════════════════════════════════════════

        public async Task<CashTransactionListViewModel> GetTransactionsAsync(
            int? cashRegisterId = null,
            string? search = null,
            TransactionType? type = null,
            TransactionCategory? category = null,
            PaymentMethod? paymentMethod = null,
            int? supplierId = null,
            int? customerId = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            bool? isCancelled = null,
            int page = 1,
            int pageSize = 50)
        {
            var query = _context.CashTransactions
                .Include(t => t.Supplier)
                .Include(t => t.Customer)
                .Include(t => t.CashRegister)
                .AsQueryable();

            // الفلاتر
            if (cashRegisterId.HasValue)
                query = query.Where(t => t.CashRegisterId == cashRegisterId.Value);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t =>
                    t.TransactionNumber.Contains(search) ||
                    t.Description.Contains(search) ||
                    (t.ReferenceNumber != null && t.ReferenceNumber.Contains(search)));
            }

            if (type.HasValue)
                query = query.Where(t => t.Type == type.Value);

            if (category.HasValue)
                query = query.Where(t => t.Category == category.Value);

            if (paymentMethod.HasValue)
                query = query.Where(t => t.PaymentMethod == paymentMethod.Value);

            if (supplierId.HasValue)
                query = query.Where(t => t.SupplierId == supplierId.Value);

            if (customerId.HasValue)
                query = query.Where(t => t.CustomerId == customerId.Value);

            if (dateFrom.HasValue)
                query = query.Where(t => t.TransactionDate >= dateFrom.Value);

            if (dateTo.HasValue)
            {
                var endDate = dateTo.Value.Date.AddDays(1);
                query = query.Where(t => t.TransactionDate < endDate);
            }

            if (isCancelled.HasValue)
                query = query.Where(t => t.IsCancelled == isCancelled.Value);

            // العدد
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Pagination
            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // الإحصائيات
            var activeTransactions = await query.Where(t => !t.IsCancelled).ToListAsync();
            var totalIncome = activeTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var totalExpense = activeTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            // الموردين والعملاء
            var suppliers = await _context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
            var customers = await _context.Customers.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();

            return new CashTransactionListViewModel
            {
                Transactions = transactions,
                Suppliers = suppliers,
                Customers = customers,
                SearchTerm = search,
                Type = type,
                Category = category,
                PaymentMethod = paymentMethod,
                SupplierId = supplierId,
                CustomerId = customerId,
                DateFrom = dateFrom,
                DateTo = dateTo,
                IsCancelled = isCancelled,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                IncomeCount = activeTransactions.Count(t => t.Type == TransactionType.Income),
                ExpenseCount = activeTransactions.Count(t => t.Type == TransactionType.Expense),
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            };
        }

        public async Task<CashTransaction?> GetTransactionByIdAsync(int id)
        {
            return await _context.CashTransactions
                .Include(t => t.Supplier)
                .Include(t => t.Customer)
                .Include(t => t.CashRegister)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // ═══════════════════════════════════════════════════
        // 🔒 الجرد اليومي
        // ═══════════════════════════════════════════════════

        public async Task<DailyClosureViewModel> PrepareDailyClosureAsync(
            int? cashRegisterId = null,
            DateTime? date = null)
        {
            var register = cashRegisterId.HasValue
                ? await _context.CashRegisters.FindAsync(cashRegisterId.Value)
                : await GetDefaultRegisterAsync();

            if (register == null)
                throw new Exception("الصندوق غير موجود");

            var closureDate = (date ?? DateTime.Today).Date;

            // التحقق من عدم وجود جرد لهذا اليوم
            var existingClosure = await _context.DailyClosures
                .FirstOrDefaultAsync(c => c.CashRegisterId == register.Id && c.ClosureDate == closureDate);

            if (existingClosure != null && existingClosure.IsClosed)
                throw new Exception("يوم الجرد مغلق بالفعل");

            // الرصيد الافتتاحي لليوم (من جرد اليوم السابق أو الرصيد الافتتاحي للصندوق)
            var previousClosure = await _context.DailyClosures
                .Where(c => c.CashRegisterId == register.Id && c.ClosureDate < closureDate)
                .OrderByDescending(c => c.ClosureDate)
                .FirstOrDefaultAsync();

            decimal openingBalance = previousClosure?.ActualBalance ?? register.OpeningBalance;

            // حركات اليوم
            var dayTransactions = await _context.CashTransactions
                .Where(t => t.CashRegisterId == register.Id
                         && t.TransactionDate.Date == closureDate
                         && !t.IsCancelled)
                .ToListAsync();

            var totalIncome = dayTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var totalExpense = dayTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            var expectedBalance = openingBalance + totalIncome - totalExpense;

            return new DailyClosureViewModel
            {
                Id = existingClosure?.Id,
                CashRegisterId = register.Id,
                CashRegisterName = register.Name,
                ClosureDate = closureDate,
                OpeningBalance = openingBalance,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                ExpectedBalance = expectedBalance,
                Count2000 = existingClosure?.Count2000 ?? 0,
                Count1000 = existingClosure?.Count1000 ?? 0,
                Count500 = existingClosure?.Count500 ?? 0,
                Count200 = existingClosure?.Count200 ?? 0,
                Count100 = existingClosure?.Count100 ?? 0,
                Count50 = existingClosure?.Count50 ?? 0,
                Count20 = existingClosure?.Count20 ?? 0,
                Count10 = existingClosure?.Count10 ?? 0,
                Count5 = existingClosure?.Count5 ?? 0,
                CoinsAmount = existingClosure?.CoinsAmount ?? 0,
                Notes = existingClosure?.Notes
            };
        }

        public async Task<(bool Success, string Message, int? ClosureId)> PerformDailyClosureAsync(
            DailyClosureViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var register = await _context.CashRegisters.FindAsync(model.CashRegisterId);
                if (register == null)
                    return (false, "الصندوق غير موجود", null);

                var closureDate = model.ClosureDate.Date;

                // البحث عن جرد موجود
                var closure = await _context.DailyClosures
                    .FirstOrDefaultAsync(c => c.CashRegisterId == model.CashRegisterId
                                           && c.ClosureDate == closureDate);

                if (closure != null && closure.IsClosed)
                    return (false, "اليوم مغلق بالفعل", null);

                var currentUser = await GetCurrentUserAsync();

                // حسابات
                var actualBalance = (model.Count2000 * 2000m) + (model.Count1000 * 1000m)
                                  + (model.Count500 * 500m) + (model.Count200 * 200m)
                                  + (model.Count100 * 100m) + (model.Count50 * 50m)
                                  + (model.Count20 * 20m) + (model.Count10 * 10m)
                                  + (model.Count5 * 5m) + model.CoinsAmount;

                var difference = actualBalance - model.ExpectedBalance;

                if (closure == null)
                {
                    closure = new DailyClosure
                    {
                        CashRegisterId = model.CashRegisterId,
                        ClosureDate = closureDate,
                        OpeningBalance = model.OpeningBalance,
                        TotalIncome = model.TotalIncome,
                        TotalExpense = model.TotalExpense,
                        ExpectedBalance = model.ExpectedBalance,
                        CreatedAt = DateTime.Now
                    };
                    _context.DailyClosures.Add(closure);
                }

                // تحديث القيم
                closure.ActualBalance = actualBalance;
                closure.Difference = difference;
                closure.Count2000 = model.Count2000;
                closure.Count1000 = model.Count1000;
                closure.Count500 = model.Count500;
                closure.Count200 = model.Count200;
                closure.Count100 = model.Count100;
                closure.Count50 = model.Count50;
                closure.Count20 = model.Count20;
                closure.Count10 = model.Count10;
                closure.Count5 = model.Count5;
                closure.CoinsAmount = model.CoinsAmount;
                closure.Notes = model.Notes;
                closure.DifferenceReason = model.DifferenceReason;
                closure.UpdatedAt = DateTime.Now;

                // إحصائيات
                var dayTransactions = await _context.CashTransactions
                    .Where(t => t.CashRegisterId == model.CashRegisterId
                             && t.TransactionDate.Date == closureDate
                             && !t.IsCancelled)
                    .ToListAsync();

                closure.TransactionsCount = dayTransactions.Count;
                closure.IncomeCount = dayTransactions.Count(t => t.Type == TransactionType.Income);
                closure.ExpenseCount = dayTransactions.Count(t => t.Type == TransactionType.Expense);

                // إذا تم اختيار الإغلاق النهائي
                if (model.CloseDay)
                {
                    closure.IsClosed = true;
                    closure.ClosedAt = DateTime.Now;
                    closure.ClosedById = currentUser?.Id;
                    closure.ClosedByName = currentUser?.FullName ?? currentUser?.UserName;
                }

                await _context.SaveChangesAsync();

                // Activity Log
                await _activityLogService.LogAsync(
                    actionType: "DailyClosure",
                    actionName: model.CloseDay ? "إغلاق يوم الصندوق" : "حفظ جرد مؤقت",
                    module: "CashRegister",
                    entityName: "DailyClosure",
                    entityId: closure.Id,
                    description: $"جرد {closureDate:yyyy-MM-dd} - الرصيد المتوقع: {model.ExpectedBalance:N2}، الفعلي: {actualBalance:N2}، الفرق: {difference:N2}",
                    severity: model.CloseDay ? "Info" : "Info");

                await transaction.CommitAsync();

                return (true,
                    model.CloseDay
                        ? $"تم إغلاق اليوم بنجاح - الفرق: {difference:N2} دج"
                        : "تم حفظ الجرد بنجاح",
                    closure.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _activityLogService.LogErrorAsync(
                    actionName: "جرد يومي",
                    errorMessage: ex.Message,
                    module: "CashRegister");
                return (false, $"حدث خطأ: {ex.Message}", null);
            }
        }

        public async Task<DailyClosureListViewModel> GetClosuresAsync(
            int? cashRegisterId = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            bool? hasDifferenceOnly = null)
        {
            var query = _context.DailyClosures
                .Include(c => c.CashRegister)
                .AsQueryable();

            if (cashRegisterId.HasValue)
                query = query.Where(c => c.CashRegisterId == cashRegisterId.Value);

            if (dateFrom.HasValue)
                query = query.Where(c => c.ClosureDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(c => c.ClosureDate <= dateTo.Value);

            if (hasDifferenceOnly == true)
                query = query.Where(c => c.Difference != 0);

            var closures = await query
                .OrderByDescending(c => c.ClosureDate)
                .ToListAsync();

            return new DailyClosureListViewModel
            {
                Closures = closures,
                CashRegisterId = cashRegisterId,
                DateFrom = dateFrom,
                DateTo = dateTo,
                HasDifferenceOnly = hasDifferenceOnly,
                TotalCount = closures.Count,
                ClosedCount = closures.Count(c => c.IsClosed),
                WithDifferenceCount = closures.Count(c => c.Difference != 0),
                TotalDifferences = closures.Sum(c => Math.Abs(c.Difference))
            };
        }

        public async Task<DailyClosure?> GetClosureByIdAsync(int id)
        {
            return await _context.DailyClosures
                .Include(c => c.CashRegister)
                .Include(c => c.Transactions)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> IsDayClosedAsync(int cashRegisterId, DateTime date)
        {
            return await _context.DailyClosures
                .AnyAsync(c => c.CashRegisterId == cashRegisterId
                            && c.ClosureDate == date.Date
                            && c.IsClosed);
        }

        // ═══════════════════════════════════════════════════
        // 📊 التقارير
        // ═══════════════════════════════════════════════════

        public async Task<CashReportViewModel> GenerateReportAsync(
            DateTime dateFrom,
            DateTime dateTo,
            int? cashRegisterId = null,
            string reportType = "custom")
        {
            var register = cashRegisterId.HasValue
                ? await _context.CashRegisters.FindAsync(cashRegisterId.Value)
                : await GetDefaultRegisterAsync();

            if (register == null)
                return new CashReportViewModel();

            var endDate = dateTo.Date.AddDays(1);

            var transactions = await _context.CashTransactions
                .Include(t => t.Supplier)
                .Include(t => t.Customer)
                .Where(t => t.CashRegisterId == register.Id
                         && t.TransactionDate >= dateFrom
                         && t.TransactionDate < endDate
                         && !t.IsCancelled)
                .OrderBy(t => t.TransactionDate)
                .ToListAsync();

            var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            // حسب الفئة - واردات
            var incomeByCategory = transactions
                .Where(t => t.Type == TransactionType.Income)
                .GroupBy(t => t.Category)
                .Select(g => new CategoryReportItem
                {
                    Category = g.Key,
                    TotalAmount = g.Sum(t => t.Amount),
                    Count = g.Count(),
                    Percentage = totalIncome > 0 ? Math.Round((g.Sum(t => t.Amount) / totalIncome) * 100, 2) : 0
                })
                .OrderByDescending(c => c.TotalAmount)
                .ToList();

            // حسب الفئة - صادرات
            var expenseByCategory = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category)
                .Select(g => new CategoryReportItem
                {
                    Category = g.Key,
                    TotalAmount = g.Sum(t => t.Amount),
                    Count = g.Count(),
                    Percentage = totalExpense > 0 ? Math.Round((g.Sum(t => t.Amount) / totalExpense) * 100, 2) : 0
                })
                .OrderByDescending(c => c.TotalAmount)
                .ToList();

            // حسب اليوم
            var dailyBreakdown = transactions
                .GroupBy(t => t.TransactionDate.Date)
                .Select(g => new DailyReportItem
                {
                    Date = g.Key,
                    Income = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    Expense = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
                    TransactionsCount = g.Count(),
                    ClosingBalance = g.LastOrDefault()?.BalanceAfter ?? 0
                })
                .OrderBy(d => d.Date)
                .ToList();

            // أعلى الموردين
            var topSuppliers = transactions
                .Where(t => t.SupplierId.HasValue && t.Type == TransactionType.Expense)
                .GroupBy(t => new { t.SupplierId, t.Supplier!.Name })
                .Select(g => new PartyReportItem
                {
                    Id = g.Key.SupplierId!.Value,
                    Name = g.Key.Name,
                    TotalAmount = g.Sum(t => t.Amount),
                    TransactionsCount = g.Count()
                })
                .OrderByDescending(s => s.TotalAmount)
                .Take(10)
                .ToList();

            // أعلى العملاء
            var topCustomers = transactions
                .Where(t => t.CustomerId.HasValue && t.Type == TransactionType.Income)
                .GroupBy(t => new { t.CustomerId, t.Customer!.Name })
                .Select(g => new PartyReportItem
                {
                    Id = g.Key.CustomerId!.Value,
                    Name = g.Key.Name,
                    TotalAmount = g.Sum(t => t.Amount),
                    TransactionsCount = g.Count()
                })
                .OrderByDescending(c => c.TotalAmount)
                .Take(10)
                .ToList();

            // طرق الدفع
            var paymentMethodStats = transactions
                .GroupBy(t => t.PaymentMethod)
                .Select(g => new PaymentMethodStats
                {
                    Method = g.Key,
                    TotalAmount = g.Sum(t => t.Amount),
                    Count = g.Count(),
                    Percentage = transactions.Any()
                        ? Math.Round(((decimal)g.Count() / transactions.Count) * 100, 2)
                        : 0
                })
                .ToList();

            return new CashReportViewModel
            {
                Title = $"تقرير الصندوق من {dateFrom:yyyy-MM-dd} إلى {dateTo:yyyy-MM-dd}",
                DateFrom = dateFrom,
                DateTo = dateTo,
                ReportType = reportType,
                CashRegister = register,
                OpeningBalance = transactions.FirstOrDefault()?.BalanceBefore ?? register.CurrentBalance,
                ClosingBalance = transactions.LastOrDefault()?.BalanceAfter ?? register.CurrentBalance,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                IncomeCount = transactions.Count(t => t.Type == TransactionType.Income),
                ExpenseCount = transactions.Count(t => t.Type == TransactionType.Expense),
                IncomeByCategory = incomeByCategory,
                ExpenseByCategory = expenseByCategory,
                DailyBreakdown = dailyBreakdown,
                TopSuppliers = topSuppliers,
                TopCustomers = topCustomers,
                Transactions = transactions,
                PaymentMethodsStats = paymentMethodStats
            };
        }

        public async Task<CashReportViewModel> GetDailyReportAsync(DateTime date, int? cashRegisterId = null)
        {
            return await GenerateReportAsync(date.Date, date.Date, cashRegisterId, "daily");
        }

        public async Task<CashReportViewModel> GetMonthlyReportAsync(int year, int month, int? cashRegisterId = null)
        {
            var dateFrom = new DateTime(year, month, 1);
            var dateTo = dateFrom.AddMonths(1).AddDays(-1);
            return await GenerateReportAsync(dateFrom, dateTo, cashRegisterId, "monthly");
        }

        public async Task<CashReportViewModel> GetYearlyReportAsync(int year, int? cashRegisterId = null)
        {
            var dateFrom = new DateTime(year, 1, 1);
            var dateTo = new DateTime(year, 12, 31);
            return await GenerateReportAsync(dateFrom, dateTo, cashRegisterId, "yearly");
        }

        // ═══════════════════════════════════════════════════
        // 🛠️ Helpers
        // ═══════════════════════════════════════════════════

        public async Task<string> GenerateTransactionNumberAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"TRX-{year}-";

            var lastNumber = await _context.CashTransactions
                .Where(t => t.TransactionNumber.StartsWith(prefix))
                .OrderByDescending(t => t.TransactionNumber)
                .Select(t => t.TransactionNumber)
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

        public async Task<(bool Success, string Message)> SetOpeningBalanceAsync(
            int cashRegisterId,
            decimal openingBalance)
        {
            try
            {
                var register = await _context.CashRegisters.FindAsync(cashRegisterId);
                if (register == null)
                    return (false, "الصندوق غير موجود");

                // التحقق من عدم وجود حركات
                var hasTransactions = await _context.CashTransactions
                    .AnyAsync(t => t.CashRegisterId == cashRegisterId);

                if (hasTransactions)
                    return (false, "لا يمكن تعديل الرصيد الافتتاحي - يوجد حركات على الصندوق");

                register.OpeningBalance = openingBalance;
                register.CurrentBalance = openingBalance;
                register.OpeningBalanceDate = DateTime.Now;
                register.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                await _activityLogService.LogAsync(
                    actionType: "SetOpeningBalance",
                    actionName: "تحديد الرصيد الافتتاحي",
                    module: "CashRegister",
                    entityName: "CashRegister",
                    entityId: cashRegisterId,
                    description: $"تحديد الرصيد الافتتاحي للصندوق: {openingBalance:N2} دج",
                    severity: "Critical");

                return (true, $"تم تحديد الرصيد الافتتاحي بنجاح: {openingBalance:N2} دج");
            }
            catch (Exception ex)
            {
                return (false, $"حدث خطأ: {ex.Message}");
            }
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