using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmarterGros.Data;
using SmarterGros.Models;
using SmarterGros.Models.Enums;
using SmarterGros.ViewModels;

namespace SmarterGros.Services
{
    /// <summary>
    /// 📊 خدمة لوحة التحكم - تجميع كل البيانات
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        // ═══════════════════════════════════════════════════
        // 🎯 الدالة الرئيسية
        // ═══════════════════════════════════════════════════

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var model = new DashboardViewModel();

            // معلومات المستخدم
            await LoadUserInfoAsync(model);

            // الأداء المالي
            await LoadFinancialPerformanceAsync(model);

            // KPIs
            await LoadKpisAsync(model);

            // الرسوم البيانية
            await LoadChartsDataAsync(model);

            // أعلى المنتجات والعملاء
            await LoadTopItemsAsync(model);

            // آخر العمليات
            await LoadRecentActivitiesAsync(model);

            // التنبيهات
            await LoadAlertsAsync(model);

            return model;
        }

        // ═══════════════════════════════════════════════════
        // 👤 معلومات المستخدم
        // ═══════════════════════════════════════════════════

        private async Task LoadUserInfoAsync(DashboardViewModel model)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var appUser = await _userManager.GetUserAsync(user);
                if (appUser != null)
                {
                    model.UserFullName = appUser.FullName ?? appUser.UserName ?? "مستخدم";
                    model.UserRole = appUser.Role ?? "موظف";
                }
            }
        }

        // ═══════════════════════════════════════════════════
        // 💰 الأداء المالي
        // ═══════════════════════════════════════════════════

        private async Task LoadFinancialPerformanceAsync(DashboardViewModel model)
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var yearStart = new DateTime(today.Year, 1, 1);

            // اليوم
            model.TodayPerformance = await CalculatePerformanceAsync(today, today.AddDays(1));

            // الأمس
            model.YesterdayPerformance = await CalculatePerformanceAsync(yesterday, today);

            // الشهر
            model.MonthPerformance = await CalculatePerformanceAsync(monthStart, today.AddDays(1));

            // السنة
            model.YearPerformance = await CalculatePerformanceAsync(yearStart, today.AddDays(1));
        }

        private async Task<FinancialPerformance> CalculatePerformanceAsync(DateTime from, DateTime to)
        {
            // المبيعات
            var sales = await _context.Sales
                .Where(s => s.SaleDate >= from && s.SaleDate < to && s.Status != InvoiceStatus.Cancelled)
                .ToListAsync();

            // المشتريات
            var purchases = await _context.Purchases
                .Where(p => p.PurchaseDate >= from && p.PurchaseDate < to && p.Status != InvoiceStatus.Cancelled)
                .ToListAsync();

            // المصاريف
            var expenses = await _context.CashTransactions
                .Where(t => t.TransactionDate >= from && t.TransactionDate < to
                         && t.Type == TransactionType.Expense
                         && !t.IsCancelled
                         && t.Category != TransactionCategory.Purchase
                         && t.Category != TransactionCategory.SupplierPayment)
                .SumAsync(t => t.Amount);

            // التحصيلات
            var collections = await _context.CashTransactions
                .Where(t => t.TransactionDate >= from && t.TransactionDate < to
                         && t.Type == TransactionType.Income
                         && !t.IsCancelled)
                .SumAsync(t => t.Amount);

            return new FinancialPerformance
            {
                SalesAmount = sales.Sum(s => s.TotalAmount),
                PurchasesAmount = purchases.Sum(p => p.TotalAmount),
                ExpensesAmount = expenses,
                CollectedAmount = collections,
                NetProfit = sales.Sum(s => s.TotalProfit) - expenses,
                SalesCount = sales.Count,
                PurchasesCount = purchases.Count
            };
        }

        // ═══════════════════════════════════════════════════
        // 📊 KPIs
        // ═══════════════════════════════════════════════════

        private async Task LoadKpisAsync(DashboardViewModel model)
        {
            // ✅ حساب التواريخ خارج Queries
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var expiringDate = today.AddDays(30);

            // عدد المنتجات
            var productsCount = await _context.Products.CountAsync(p => p.IsActive);
            var lowStockCount = await _context.Products
                .CountAsync(p => p.IsActive && p.StockQuantity <= p.MinStockAlert && p.StockQuantity > 0);
            var outOfStockCount = await _context.Products
                .CountAsync(p => p.IsActive && p.StockQuantity == 0);

            model.ProductsCount = new KpiCard
            {
                Title = "المنتجات",
                Value = productsCount.ToString("N0"),
                Subtitle = $"{lowStockCount} منخفض | {outOfStockCount} نافذ",
                Icon = "fa-box",
                Color = "primary",
                ActionUrl = "/Products"
            };

            // عدد العملاء
            var customersCount = await _context.Customers.CountAsync(c => c.IsActive);
            var newCustomersThisMonth = await _context.Customers
                .CountAsync(c => c.IsActive && c.CreatedAt >= monthStart);

            model.CustomersCount = new KpiCard
            {
                Title = "العملاء",
                Value = customersCount.ToString("N0"),
                Subtitle = $"+{newCustomersThisMonth} هذا الشهر",
                Icon = "fa-users",
                Color = "info",
                ActionUrl = "/Customers"
            };

            // عدد الموردين
            var suppliersCount = await _context.Suppliers.CountAsync(s => s.IsActive);

            model.SuppliersCount = new KpiCard
            {
                Title = "الموردون",
                Value = suppliersCount.ToString("N0"),
                Subtitle = "نشط",
                Icon = "fa-truck",
                Color = "warning",
                ActionUrl = "/Suppliers"
            };

            // رصيد الصندوق
            var register = await _context.CashRegisters.FirstOrDefaultAsync(c => c.IsDefault);
            var cashBalance = register?.CurrentBalance ?? 0;

            model.CashBalance = new KpiCard
            {
                Title = "رصيد الصندوق",
                Value = cashBalance.ToString("N0") + " دج",
                Subtitle = register?.Name ?? "الصندوق",
                Icon = "fa-cash-register",
                Color = "success",
                ActionUrl = "/CashRegister"
            };

            // ديون العملاء
            var customersDebt = await _context.Sales
                .Where(s => s.Status != InvoiceStatus.Cancelled && s.RemainingAmount > 0)
                .SumAsync(s => s.RemainingAmount);

            var unpaidSalesCount = await _context.Sales
                .CountAsync(s => s.Status != InvoiceStatus.Cancelled && s.RemainingAmount > 0);

            model.CustomersDebt = new KpiCard
            {
                Title = "ديون العملاء (لنا)",
                Value = customersDebt.ToString("N0") + " دج",
                Subtitle = $"{unpaidSalesCount} فاتورة غير مسددة",
                Icon = "fa-hand-holding-usd",
                Color = "success",
                IsPositive = true
            };

            // ديون الموردين
            var suppliersDebt = await _context.Purchases
                .Where(p => p.Status != InvoiceStatus.Cancelled && p.RemainingAmount > 0)
                .SumAsync(p => p.RemainingAmount);

            var unpaidPurchasesCount = await _context.Purchases
                .CountAsync(p => p.Status != InvoiceStatus.Cancelled && p.RemainingAmount > 0);

            model.SuppliersDebt = new KpiCard
            {
                Title = "ديون الموردين (علينا)",
                Value = suppliersDebt.ToString("N0") + " دج",
                Subtitle = $"{unpaidPurchasesCount} فاتورة مستحقة",
                Icon = "fa-credit-card",
                Color = "danger",
                IsPositive = false
            };

            // المنتجات المنخفضة
            model.LowStockCount = new KpiCard
            {
                Title = "منخفض المخزون",
                Value = lowStockCount.ToString(),
                Subtitle = "يحتاج تجديد",
                Icon = "fa-exclamation-triangle",
                Color = "warning",
                ActionUrl = "/Products"
            };

            // المنتجات قرب الانتهاء
            var expiringCount = await _context.Products
                .CountAsync(p => p.IsActive && p.ExpiryDate.HasValue && p.ExpiryDate <= expiringDate);

            model.ExpiringProductsCount = new KpiCard
            {
                Title = "قرب الانتهاء",
                Value = expiringCount.ToString(),
                Subtitle = "خلال 30 يوم",
                Icon = "fa-clock",
                Color = "warning",
                ActionUrl = "/Products"
            };
        }

        // ═══════════════════════════════════════════════════
        // 📈 الرسوم البيانية
        // ═══════════════════════════════════════════════════

        private async Task LoadChartsDataAsync(DashboardViewModel model)
        {
            // آخر 7 أيام (المبيعات vs المشتريات)
            model.Last7DaysData = await GetLast7DaysDataAsync();

            // آخر 30 يوم (الأرباح)
            model.Last30DaysProfit = await GetLast30DaysProfitAsync();

            // توزيع المبيعات حسب الفئة (هذا الشهر)
            model.SalesByCategory = await GetSalesByCategoryAsync();

            // تدفق الصندوق الشهري
            model.CashFlowMonth = await GetCashFlowMonthAsync();
        }

        private async Task<List<DailyTransactionData>> GetLast7DaysDataAsync()
        {
            var startDate = DateTime.Today.AddDays(-6);
            var result = new List<DailyTransactionData>();

            for (int i = 0; i < 7; i++)
            {
                var date = startDate.AddDays(i);
                var nextDate = date.AddDays(1);

                var sales = await _context.Sales
                    .Where(s => s.SaleDate >= date && s.SaleDate < nextDate
                             && s.Status != InvoiceStatus.Cancelled)
                    .ToListAsync();

                var purchases = await _context.Purchases
                    .Where(p => p.PurchaseDate >= date && p.PurchaseDate < nextDate
                             && p.Status != InvoiceStatus.Cancelled)
                    .ToListAsync();

                result.Add(new DailyTransactionData
                {
                    Date = date,
                    SalesAmount = sales.Sum(s => s.TotalAmount),
                    PurchasesAmount = purchases.Sum(p => p.TotalAmount),
                    SalesCount = sales.Count,
                    PurchasesCount = purchases.Count
                });
            }

            return result;
        }

        private async Task<List<DailyProfitData>> GetLast30DaysProfitAsync()
        {
            var startDate = DateTime.Today.AddDays(-29);
            var result = new List<DailyProfitData>();

            for (int i = 0; i < 30; i++)
            {
                var date = startDate.AddDays(i);
                var nextDate = date.AddDays(1);

                var sales = await _context.Sales
                    .Where(s => s.SaleDate >= date && s.SaleDate < nextDate
                             && s.Status != InvoiceStatus.Cancelled)
                    .ToListAsync();

                result.Add(new DailyProfitData
                {
                    Date = date,
                    Profit = sales.Sum(s => s.TotalProfit),
                    Revenue = sales.Sum(s => s.TotalAmount),
                    Cost = sales.Sum(s => s.TotalCost)
                });
            }

            return result;
        }

        private async Task<List<CategoryDistribution>> GetSalesByCategoryAsync()
        {
            var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var nextMonth = monthStart.AddMonths(1);

            var data = await _context.SaleItems
                .Include(si => si.Sale)
                .Include(si => si.Product)
                    .ThenInclude(p => p!.Category)
                .Where(si => si.Sale!.SaleDate >= monthStart
                          && si.Sale.SaleDate < nextMonth
                          && si.Sale.Status != InvoiceStatus.Cancelled)
                .GroupBy(si => new { si.Product!.Category!.Id, si.Product.Category.Name })
                .Select(g => new
                {
                    CategoryName = g.Key.Name,
                    Amount = g.Sum(si => si.TotalPrice),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Amount)
                .Take(8)
                .ToListAsync();

            var total = data.Sum(d => d.Amount);
            var colors = new[] { "#0E4D3A", "#d32f2f", "#1976d2", "#7b1fa2", "#f57c00",
                                "#388e3c", "#c2185b", "#0288d1" };

            return data.Select((d, i) => new CategoryDistribution
            {
                CategoryName = d.CategoryName,
                Amount = d.Amount,
                Count = d.Count,
                Percentage = total > 0 ? Math.Round((d.Amount / total) * 100, 2) : 0,
                Color = colors[i % colors.Length]
            }).ToList();
        }

        private async Task<List<CashFlowData>> GetCashFlowMonthAsync()
        {
            var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            var today = DateTime.Today;

            var result = new List<CashFlowData>();
            decimal runningBalance = 0;

            for (int i = 0; i < Math.Min(daysInMonth, today.Day); i++)
            {
                var date = monthStart.AddDays(i);
                var nextDate = date.AddDays(1);

                var income = await _context.CashTransactions
                    .Where(t => t.TransactionDate >= date && t.TransactionDate < nextDate
                             && t.Type == TransactionType.Income && !t.IsCancelled)
                    .SumAsync(t => t.Amount);

                var expense = await _context.CashTransactions
                    .Where(t => t.TransactionDate >= date && t.TransactionDate < nextDate
                             && t.Type == TransactionType.Expense && !t.IsCancelled)
                    .SumAsync(t => t.Amount);

                runningBalance += income - expense;

                result.Add(new CashFlowData
                {
                    Date = date,
                    Income = income,
                    Expense = expense,
                    Balance = runningBalance
                });
            }

            return result;
        }

        // ═══════════════════════════════════════════════════
        // 🏆 أعلى المنتجات والعملاء
        // ═══════════════════════════════════════════════════

        private async Task LoadTopItemsAsync(DashboardViewModel model)
        {
            var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            // أعلى المنتجات
            model.TopSellingProducts = await _context.SaleItems
                .Include(si => si.Sale)
                .Include(si => si.Product)
                .Where(si => si.Sale!.SaleDate >= monthStart
                          && si.Sale.Status != InvoiceStatus.Cancelled)
                .GroupBy(si => new { si.ProductId, si.Product!.Name, si.Product.Reference, si.Product.StockQuantity })
                .Select(g => new TopProduct
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    Reference = g.Key.Reference,
                    QuantitySold = g.Sum(si => si.Quantity),
                    TotalSales = g.Sum(si => si.TotalPrice),
                    TotalProfit = g.Sum(si => si.Profit),
                    CurrentStock = g.Key.StockQuantity
                })
                .OrderByDescending(p => p.QuantitySold)
                .Take(5)
                .ToListAsync();

            // أعلى العملاء
            model.TopCustomers = await _context.Sales
                .Include(s => s.Customer)
                .Where(s => s.SaleDate >= monthStart
                         && s.Status != InvoiceStatus.Cancelled
                         && s.CustomerId.HasValue)
                .GroupBy(s => new { s.CustomerId, s.Customer!.Name })
                .Select(g => new TopCustomer
                {
                    CustomerId = g.Key.CustomerId!.Value,
                    CustomerName = g.Key.Name,
                    TotalPurchases = g.Sum(s => s.TotalAmount),
                    InvoicesCount = g.Count(),
                    CurrentDebt = g.Sum(s => s.RemainingAmount)
                })
                .OrderByDescending(c => c.TotalPurchases)
                .Take(5)
                .ToListAsync();

            // أعلى الموردين
            model.TopSuppliers = await _context.Purchases
                .Include(p => p.Supplier)
                .Where(p => p.PurchaseDate >= monthStart
                         && p.Status != InvoiceStatus.Cancelled)
                .GroupBy(p => new { p.SupplierId, p.Supplier!.Name })
                .Select(g => new TopSupplier
                {
                    SupplierId = g.Key.SupplierId,
                    SupplierName = g.Key.Name,
                    TotalPurchases = g.Sum(p => p.TotalAmount),
                    InvoicesCount = g.Count(),
                    CurrentDebt = g.Sum(p => p.RemainingAmount)
                })
                .OrderByDescending(s => s.TotalPurchases)
                .Take(5)
                .ToListAsync();
        }

        // ═══════════════════════════════════════════════════
        // 📋 آخر العمليات
        // ═══════════════════════════════════════════════════

        private async Task LoadRecentActivitiesAsync(DashboardViewModel model)
        {
            // آخر المبيعات
            model.RecentSales = await _context.Sales
                .Include(s => s.Customer)
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .Select(s => new RecentInvoice
                {
                    Id = s.Id,
                    InvoiceNumber = s.InvoiceNumber,
                    PartyName = s.Customer != null ? s.Customer.Name : (s.CustomerName ?? "عميل نقدي"),
                    InvoiceDate = s.SaleDate,
                    TotalAmount = s.TotalAmount,
                    Status = s.Status.GetArabicName(),
                    StatusColor = s.Status.GetBadgeColor(),
                    PaymentType = s.PaymentType.GetArabicName(),
                    Url = $"/Sales/Details/{s.Id}"
                })
                .ToListAsync();

            // آخر المشتريات
            model.RecentPurchases = await _context.Purchases
                .Include(p => p.Supplier)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new RecentInvoice
                {
                    Id = p.Id,
                    InvoiceNumber = p.InvoiceNumber,
                    PartyName = p.Supplier != null ? p.Supplier.Name : "غير معروف",
                    InvoiceDate = p.PurchaseDate,
                    TotalAmount = p.TotalAmount,
                    Status = p.Status.GetArabicName(),
                    StatusColor = p.Status.GetBadgeColor(),
                    PaymentType = p.PaymentType.GetArabicName(),
                    Url = $"/Purchases/Details/{p.Id}"
                })
                .ToListAsync();

            // آخر حركات الصندوق
            model.RecentCashTransactions = await _context.CashTransactions
                .Where(t => !t.IsCancelled)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .Select(t => new RecentCashTransaction
                {
                    Id = t.Id,
                    TransactionNumber = t.TransactionNumber,
                    TransactionDate = t.TransactionDate,
                    Description = t.Description,
                    Amount = t.Amount,
                    IsIncome = t.Type == TransactionType.Income,
                    Category = t.Category.GetArabicName(),
                    Icon = t.Category.GetIcon()
                })
                .ToListAsync();
        }

        // ═══════════════════════════════════════════════════
        // ⚠️ التنبيهات الذكية
        // ═══════════════════════════════════════════════════

        private async Task LoadAlertsAsync(DashboardViewModel model)
        {
            var alerts = new List<DashboardAlert>();

            // 1. منتجات نافذة
            var outOfStockCount = await _context.Products
                .CountAsync(p => p.IsActive && p.StockQuantity == 0);

            if (outOfStockCount > 0)
            {
                alerts.Add(new DashboardAlert
                {
                    Title = "منتجات نافذة",
                    Message = $"{outOfStockCount} منتج نافذ من المخزون - يحتاج إعادة طلب",
                    Icon = "fa-times-circle",
                    Color = "danger",
                    Priority = 1,
                    ActionUrl = "/Products",
                    ActionText = "عرض"
                });
            }

            // 2. منتجات منخفضة
            var lowStockCount = await _context.Products
                .CountAsync(p => p.IsActive && p.StockQuantity > 0 && p.StockQuantity <= p.MinStockAlert);

            if (lowStockCount > 0)
            {
                alerts.Add(new DashboardAlert
                {
                    Title = "مخزون منخفض",
                    Message = $"{lowStockCount} منتج وصل للحد الأدنى",
                    Icon = "fa-exclamation-triangle",
                    Color = "warning",
                    Priority = 2,
                    ActionUrl = "/Products",
                    ActionText = "عرض"
                });
            }

            // 3. منتجات قرب الانتهاء
            var expiringDate = DateTime.Today.AddDays(30);
            var expiringCount = await _context.Products
                .CountAsync(p => p.IsActive && p.ExpiryDate.HasValue
                              && p.ExpiryDate <= expiringDate
                              && p.ExpiryDate >= DateTime.Today);

            if (expiringCount > 0)
            {
                alerts.Add(new DashboardAlert
                {
                    Title = "منتجات قرب الانتهاء",
                    Message = $"{expiringCount} منتج ينتهي خلال 30 يوم",
                    Icon = "fa-clock",
                    Color = "warning",
                    Priority = 2,
                    ActionUrl = "/Products",
                    ActionText = "عرض"
                });
            }

            // 4. منتجات منتهية الصلاحية
            var expiredCount = await _context.Products
                .CountAsync(p => p.IsActive && p.ExpiryDate.HasValue && p.ExpiryDate < DateTime.Today);

            if (expiredCount > 0)
            {
                alerts.Add(new DashboardAlert
                {
                    Title = "منتجات منتهية الصلاحية!",
                    Message = $"{expiredCount} منتج منتهي الصلاحية - يجب حذفه",
                    Icon = "fa-skull-crossbones",
                    Color = "danger",
                    Priority = 1,
                    ActionUrl = "/Products",
                    ActionText = "معالجة"
                });
            }

            // 5. ديون متأخرة (أكثر من 30 يوم)
            var debtCutoffDate = DateTime.Today.AddDays(-30);
            var oldDebtsCustomers = await _context.Sales
                .CountAsync(s => s.Status != InvoiceStatus.Cancelled
                              && s.RemainingAmount > 0
                              && s.SaleDate < debtCutoffDate);

            if (oldDebtsCustomers > 0)
            {
                alerts.Add(new DashboardAlert
                {
                    Title = "ديون عملاء متأخرة",
                    Message = $"{oldDebtsCustomers} فاتورة بيع متأخرة أكثر من 30 يوم",
                    Icon = "fa-calendar-times",
                    Color = "warning",
                    Priority = 2,
                    ActionUrl = "/Sales",
                    ActionText = "متابعة"
                });
            }

            // 6. اليوم لم يُغلق (الصندوق)
            var todayDate = DateTime.Today;
            var todayClosed = await _context.DailyClosures
                .AnyAsync(c => c.ClosureDate == todayDate && c.IsClosed);

            if (!todayClosed && DateTime.Now.Hour >= 18)
            {
                alerts.Add(new DashboardAlert
                {
                    Title = "لم يتم إغلاق اليوم",
                    Message = "يجب تنفيذ الجرد اليومي للصندوق",
                    Icon = "fa-lock-open",
                    Color = "info",
                    Priority = 3,
                    ActionUrl = "/CashRegister/DailyClosure",
                    ActionText = "إغلاق"
                });
            }

            // 7. أداء جيد (تحفيز)
            if (model.TodayPerformance.NetProfit > model.YesterdayPerformance.NetProfit
                && model.TodayPerformance.NetProfit > 0)
            {
                var improvement = model.YesterdayPerformance.NetProfit > 0
                    ? Math.Round(((model.TodayPerformance.NetProfit - model.YesterdayPerformance.NetProfit)
                                / model.YesterdayPerformance.NetProfit) * 100, 0)
                    : 100;

                alerts.Add(new DashboardAlert
                {
                    Title = "أداء ممتاز! 🎉",
                    Message = $"أرباح اليوم تجاوزت الأمس بنسبة {improvement}%",
                    Icon = "fa-trophy",
                    Color = "success",
                    Priority = 3
                });
            }

            // ترتيب التنبيهات حسب الأولوية
            model.Alerts = alerts.OrderBy(a => a.Priority).Take(6).ToList();
        }
    }
}