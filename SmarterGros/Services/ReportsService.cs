using Microsoft.EntityFrameworkCore;
using SmarterGros.Data;
using SmarterGros.Models;
using SmarterGros.Models.Enums;
using SmarterGros.ViewModels.Reports;

namespace SmarterGros.Services
{
    /// <summary>
    /// 📊 خدمة التقارير الشاملة
    /// </summary>
    public class ReportsService : IReportsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICashRegisterService _cashService;

        public ReportsService(
            ApplicationDbContext context,
            ICashRegisterService cashService)
        {
            _context = context;
            _cashService = cashService;
        }

        // ═══════════════════════════════════════════════════
        // 📊 الصفحة الرئيسية للتقارير
        // ═══════════════════════════════════════════════════

        public async Task<ReportsListViewModel> GetReportsListAsync()
        {
            var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var today = DateTime.Today;

            // ملخص الشهر
            var monthSales = await _context.Sales
                .Where(s => s.SaleDate >= monthStart && s.Status != InvoiceStatus.Cancelled)
                .ToListAsync();

            var monthPurchases = await _context.Purchases
                .Where(p => p.PurchaseDate >= monthStart && p.Status != InvoiceStatus.Cancelled)
                .ToListAsync();

            var cashBalance = await _cashService.GetCurrentBalanceAsync();

            var lowStockCount = await _context.Products
                .CountAsync(p => p.IsActive && p.StockQuantity <= p.MinStockAlert);

            return new ReportsListViewModel
            {
                TotalSalesMonth = monthSales.Sum(s => s.TotalAmount),
                TotalPurchasesMonth = monthPurchases.Sum(p => p.TotalAmount),
                TotalProfitMonth = monthSales.Sum(s => s.TotalProfit),
                TotalInvoicesMonth = monthSales.Count + monthPurchases.Count,
                CashBalance = cashBalance,
                LowStockCount = lowStockCount,

                AvailableReports = new List<ReportCardInfo>
                {
                    new() {
                        Title = "تقرير المبيعات",
                        Description = "تحليل شامل لكل عمليات البيع والأرباح",
                        Icon = "fa-cash-register",
                        Color = "danger",
                        ActionName = "SalesReport",
                        Order = 1
                    },
                    new() {
                        Title = "تقرير المشتريات",
                        Description = "تحليل عمليات الشراء من الموردين",
                        Icon = "fa-shopping-cart",
                        Color = "success",
                        ActionName = "PurchasesReport",
                        Order = 2
                    },
                    new() {
                        Title = "الأرباح والخسائر",
                        Description = "بيان مالي شامل بالإيرادات والمصاريف",
                        Icon = "fa-chart-line",
                        Color = "primary",
                        ActionName = "ProfitLossReport",
                        Order = 3
                    },
                    new() {
                        Title = "تقرير المخزون",
                        Description = "حالة المخزون وقيمته والمنتجات المنخفضة",
                        Icon = "fa-boxes",
                        Color = "warning",
                        ActionName = "InventoryReport",
                        Order = 4
                    },
                    new() {
                        Title = "العملاء والموردين",
                        Description = "تحليل الديون وأعمارها للشركاء",
                        Icon = "fa-users",
                        Color = "info",
                        ActionName = "PartnersReport",
                        Order = 5
                    },
                    new() {
                        Title = "تقرير الصندوق",
                        Description = "حركة الصندوق والإيرادات والمصاريف",
                        Icon = "fa-money-bill-wave",
                        Color = "success",
                        ActionName = "CashReport",
                        Order = 6
                    }
                }
            };
        }

        // ═══════════════════════════════════════════════════
        // 💰 تقرير المبيعات الشامل
        // ═══════════════════════════════════════════════════

        public async Task<SalesReportViewModel> GetSalesReportAsync(
            DateTime dateFrom,
            DateTime dateTo,
            int? customerId = null,
            int? categoryId = null)
        {
            var endDate = dateTo.Date.AddDays(1);
            var model = new SalesReportViewModel
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                CustomerId = customerId,
                CategoryId = categoryId,
                PeriodLabel = $"من {dateFrom:yyyy/MM/dd} إلى {dateTo:yyyy/MM/dd}"
            };

            // الفلاتر
            var query = _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                        .ThenInclude(p => p!.Category)
                .Where(s => s.SaleDate >= dateFrom && s.SaleDate < endDate
                         && s.Status != InvoiceStatus.Cancelled);

            if (customerId.HasValue)
                query = query.Where(s => s.CustomerId == customerId.Value);

            if (categoryId.HasValue)
                query = query.Where(s => s.SaleItems.Any(si => si.Product!.CategoryId == categoryId.Value));

            var sales = await query.OrderByDescending(s => s.SaleDate).ToListAsync();

            // ملخص شامل
            model.TotalSales = sales.Sum(s => s.TotalAmount);
            model.TotalCost = sales.Sum(s => s.TotalCost);
            model.TotalProfit = sales.Sum(s => s.TotalProfit);
            model.TotalTax = sales.Sum(s => s.TaxAmount);
            model.TotalDiscount = sales.Sum(s => s.Discount);
            model.TotalCollected = sales.Sum(s => s.PaidAmount);
            model.TotalDebt = sales.Sum(s => s.RemainingAmount);
            model.TotalInvoices = sales.Count;
            model.TotalItemsSold = sales.Sum(s => s.SaleItems.Sum(si => si.Quantity));
            model.AverageInvoiceValue = sales.Count > 0 ? model.TotalSales / sales.Count : 0;
            model.ProfitMargin = model.TotalSales > 0
                ? Math.Round((model.TotalProfit / model.TotalSales) * 100, 2)
                : 0;

            // المقارنة مع الفترة السابقة
            var daysDiff = (dateTo - dateFrom).Days;
            var previousFrom = dateFrom.AddDays(-daysDiff - 1);
            var previousTo = dateFrom.AddDays(-1);

            var previousSales = await _context.Sales
                .Where(s => s.SaleDate >= previousFrom && s.SaleDate <= previousTo
                         && s.Status != InvoiceStatus.Cancelled)
                .ToListAsync();

            model.PreviousPeriodSales = previousSales.Sum(s => s.TotalAmount);
            model.PreviousPeriodProfit = previousSales.Sum(s => s.TotalProfit);
            model.SalesGrowthPercent = model.PreviousPeriodSales > 0
                ? Math.Round(((model.TotalSales - model.PreviousPeriodSales) / model.PreviousPeriodSales) * 100, 2)
                : 0;
            model.ProfitGrowthPercent = model.PreviousPeriodProfit > 0
                ? Math.Round(((model.TotalProfit - model.PreviousPeriodProfit) / model.PreviousPeriodProfit) * 100, 2)
                : 0;

            // المبيعات اليومية
            model.DailySales = sales
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new DailySalesData
                {
                    Date = g.Key,
                    SalesAmount = g.Sum(s => s.TotalAmount),
                    Profit = g.Sum(s => s.TotalProfit),
                    InvoicesCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            // المبيعات حسب الفئة
            var categoryData = sales
                .SelectMany(s => s.SaleItems)
                .Where(si => si.Product?.Category != null)
                .GroupBy(si => new { si.Product!.Category!.Id, si.Product.Category.Name })
                .Select(g => new
                {
                    Name = g.Key.Name,
                    Amount = g.Sum(si => si.TotalPrice),
                    Quantity = g.Sum(si => si.Quantity)
                })
                .OrderByDescending(c => c.Amount)
                .Take(10)
                .ToList();

            var colors = new[] { "#667eea", "#f5576c", "#11998e", "#fa709a", "#4facfe",
                                 "#43e97b", "#f093fb", "#feca57", "#48dbfb", "#ff6b6b" };

            model.SalesByCategory = categoryData.Select((c, i) => new CategorySalesData
            {
                CategoryName = c.Name,
                Amount = c.Amount,
                Quantity = c.Quantity,
                Percentage = model.TotalSales > 0
                    ? Math.Round((c.Amount / model.TotalSales) * 100, 2)
                    : 0,
                Color = colors[i % colors.Length]
            }).ToList();

            // المبيعات حسب نوع الدفع
            model.SalesByPaymentType = sales
                .GroupBy(s => s.PaymentType)
                .Select(g => new PaymentTypeData
                {
                    PaymentTypeName = g.Key.GetArabicName(),
                    Amount = g.Sum(s => s.TotalAmount),
                    Count = g.Count(),
                    Percentage = model.TotalSales > 0
                        ? Math.Round((g.Sum(s => s.TotalAmount) / model.TotalSales) * 100, 2)
                        : 0
                })
                .ToList();

            // المبيعات حسب نوع السعر
            model.SalesByPriceType = sales
                .GroupBy(s => s.PriceType)
                .Select(g => new PriceTypeData
                {
                    PriceTypeName = g.Key.GetArabicName(),
                    Amount = g.Sum(s => s.TotalAmount),
                    Count = g.Count(),
                    Percentage = model.TotalSales > 0
                        ? Math.Round((g.Sum(s => s.TotalAmount) / model.TotalSales) * 100, 2)
                        : 0
                })
                .ToList();

            // أعلى المنتجات
            model.TopProducts = sales
                .SelectMany(s => s.SaleItems)
                .Where(si => si.Product != null)
                .GroupBy(si => new { si.ProductId, si.Product!.Name, si.Product.Reference })
                .Select(g => new TopProductData
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    Reference = g.Key.Reference,
                    QuantitySold = g.Sum(si => si.Quantity),
                    TotalSales = g.Sum(si => si.TotalPrice),
                    TotalProfit = g.Sum(si => si.Profit)
                })
                .OrderByDescending(p => p.QuantitySold)
                .Take(10)
                .ToList();

            // أعلى العملاء
            model.TopCustomers = sales
                .Where(s => s.CustomerId.HasValue && s.Customer != null)
                .GroupBy(s => new { s.CustomerId, s.Customer!.Name })
                .Select(g => new TopCustomerData
                {
                    CustomerId = g.Key.CustomerId!.Value,
                    CustomerName = g.Key.Name,
                    InvoicesCount = g.Count(),
                    TotalPurchases = g.Sum(s => s.TotalAmount),
                    CurrentDebt = g.Sum(s => s.RemainingAmount)
                })
                .OrderByDescending(c => c.TotalPurchases)
                .Take(10)
                .ToList();

            // الفواتير
            model.Invoices = sales;

            // Dropdowns
            model.Customers = await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            model.Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return model;
        }

        // ═══════════════════════════════════════════════════
        // 🛒 تقرير المشتريات الشامل
        // ═══════════════════════════════════════════════════

        public async Task<PurchasesReportViewModel> GetPurchasesReportAsync(
            DateTime dateFrom,
            DateTime dateTo,
            int? supplierId = null,
            int? categoryId = null)
        {
            var endDate = dateTo.Date.AddDays(1);
            var model = new PurchasesReportViewModel
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                SupplierId = supplierId,
                CategoryId = categoryId,
                PeriodLabel = $"من {dateFrom:yyyy/MM/dd} إلى {dateTo:yyyy/MM/dd}"
            };

            // الفلاتر
            var query = _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                        .ThenInclude(prod => prod!.Category)
                .Where(p => p.PurchaseDate >= dateFrom && p.PurchaseDate < endDate
                         && p.Status != InvoiceStatus.Cancelled);

            if (supplierId.HasValue)
                query = query.Where(p => p.SupplierId == supplierId.Value);

            if (categoryId.HasValue)
                query = query.Where(p => p.PurchaseItems.Any(pi => pi.Product!.CategoryId == categoryId.Value));

            var purchases = await query.OrderByDescending(p => p.PurchaseDate).ToListAsync();

            // الملخص
            model.TotalPurchases = purchases.Sum(p => p.TotalAmount);
            model.TotalTax = purchases.Sum(p => p.TaxAmount);
            model.TotalDiscount = purchases.Sum(p => p.Discount);
            model.TotalPaid = purchases.Sum(p => p.PaidAmount);
            model.TotalDebt = purchases.Sum(p => p.RemainingAmount);
            model.TotalShippingCost = purchases.Sum(p => p.ShippingCost);
            model.TotalInvoices = purchases.Count;
            model.TotalItemsPurchased = purchases.Sum(p => p.PurchaseItems.Sum(pi => pi.Quantity));
            model.AverageInvoiceValue = purchases.Count > 0 ? model.TotalPurchases / purchases.Count : 0;

            // المقارنة مع الفترة السابقة
            var daysDiff = (dateTo - dateFrom).Days;
            var previousFrom = dateFrom.AddDays(-daysDiff - 1);
            var previousTo = dateFrom.AddDays(-1);

            var previousPurchases = await _context.Purchases
                .Where(p => p.PurchaseDate >= previousFrom && p.PurchaseDate <= previousTo
                         && p.Status != InvoiceStatus.Cancelled)
                .ToListAsync();

            model.PreviousPeriodPurchases = previousPurchases.Sum(p => p.TotalAmount);
            model.PurchasesGrowthPercent = model.PreviousPeriodPurchases > 0
                ? Math.Round(((model.TotalPurchases - model.PreviousPeriodPurchases) / model.PreviousPeriodPurchases) * 100, 2)
                : 0;

            // المشتريات اليومية
            model.DailyPurchases = purchases
                .GroupBy(p => p.PurchaseDate.Date)
                .Select(g => new DailyPurchasesData
                {
                    Date = g.Key,
                    PurchasesAmount = g.Sum(p => p.TotalAmount),
                    InvoicesCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            // المشتريات حسب الفئة
            var categoryData = purchases
                .SelectMany(p => p.PurchaseItems)
                .Where(pi => pi.Product?.Category != null)
                .GroupBy(pi => new { pi.Product!.Category!.Id, pi.Product.Category.Name })
                .Select(g => new
                {
                    Name = g.Key.Name,
                    Amount = g.Sum(pi => pi.TotalPrice),
                    Quantity = g.Sum(pi => pi.Quantity)
                })
                .OrderByDescending(c => c.Amount)
                .Take(10)
                .ToList();

            var colors = new[] { "#11998e", "#667eea", "#f5576c", "#fa709a", "#4facfe",
                                 "#43e97b", "#f093fb", "#feca57", "#48dbfb", "#ff6b6b" };

            model.PurchasesByCategory = categoryData.Select((c, i) => new CategoryPurchasesData
            {
                CategoryName = c.Name,
                Amount = c.Amount,
                Quantity = c.Quantity,
                Percentage = model.TotalPurchases > 0
                    ? Math.Round((c.Amount / model.TotalPurchases) * 100, 2)
                    : 0,
                Color = colors[i % colors.Length]
            }).ToList();

            // المشتريات حسب نوع الدفع
            model.PurchasesByPaymentType = purchases
                .GroupBy(p => p.PaymentType)
                .Select(g => new PaymentTypeData
                {
                    PaymentTypeName = g.Key.GetArabicName(),
                    Amount = g.Sum(p => p.TotalAmount),
                    Count = g.Count(),
                    Percentage = model.TotalPurchases > 0
                        ? Math.Round((g.Sum(p => p.TotalAmount) / model.TotalPurchases) * 100, 2)
                        : 0
                })
                .ToList();

            // أعلى الموردين
            model.TopSuppliers = purchases
                .Where(p => p.Supplier != null)
                .GroupBy(p => new { p.SupplierId, p.Supplier!.Name })
                .Select(g => new TopSupplierData
                {
                    SupplierId = g.Key.SupplierId,
                    SupplierName = g.Key.Name,
                    InvoicesCount = g.Count(),
                    TotalPurchases = g.Sum(p => p.TotalAmount),
                    CurrentDebt = g.Sum(p => p.RemainingAmount)
                })
                .OrderByDescending(s => s.TotalPurchases)
                .Take(10)
                .ToList();

            // أعلى المنتجات المشتراة
            model.TopPurchasedProducts = purchases
                .SelectMany(p => p.PurchaseItems)
                .Where(pi => pi.Product != null)
                .GroupBy(pi => new { pi.ProductId, pi.Product!.Name, pi.Product.StockQuantity })
                .Select(g => new TopPurchasedProductData
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    QuantityPurchased = g.Sum(pi => pi.Quantity),
                    TotalCost = g.Sum(pi => pi.TotalPrice),
                    CurrentStock = g.Key.StockQuantity
                })
                .OrderByDescending(p => p.QuantityPurchased)
                .Take(10)
                .ToList();

            // الفواتير
            model.Invoices = purchases;

            // Dropdowns
            model.Suppliers = await _context.Suppliers
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            model.Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return model;
        }

        // ═══════════════════════════════════════════════════
        // 💰 تقرير الأرباح والخسائر
        // ═══════════════════════════════════════════════════

        public async Task<ProfitLossReportViewModel> GetProfitLossReportAsync(
            DateTime dateFrom,
            DateTime dateTo)
        {
            var endDate = dateTo.Date.AddDays(1);
            var model = new ProfitLossReportViewModel
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                PeriodLabel = $"من {dateFrom:yyyy/MM/dd} إلى {dateTo:yyyy/MM/dd}"
            };

            // ─── الإيرادات ───

            // إيرادات المبيعات
            var sales = await _context.Sales
                .Where(s => s.SaleDate >= dateFrom && s.SaleDate < endDate
                         && s.Status != InvoiceStatus.Cancelled)
                .ToListAsync();

            model.SalesRevenue = sales.Sum(s => s.TotalAmount);
            model.CostOfGoodsSold = sales.Sum(s => s.TotalCost);

            // التحصيلات النقدية
            model.CashCollections = await _context.CashTransactions
                .Where(t => t.TransactionDate >= dateFrom && t.TransactionDate < endDate
                         && !t.IsCancelled
                         && (t.Category == TransactionCategory.CustomerPayment))
                .SumAsync(t => t.Amount);

            // الإيرادات الأخرى
            model.OtherIncome = await _context.CashTransactions
                .Where(t => t.TransactionDate >= dateFrom && t.TransactionDate < endDate
                         && t.Type == TransactionType.Income
                         && !t.IsCancelled
                         && t.Category != TransactionCategory.Sale
                         && t.Category != TransactionCategory.CustomerPayment)
                .SumAsync(t => t.Amount);

            // ─── المصاريف ───

            // المشتريات
            var purchases = await _context.Purchases
                .Where(p => p.PurchaseDate >= dateFrom && p.PurchaseDate < endDate
                         && p.Status != InvoiceStatus.Cancelled)
                .ToListAsync();

            model.Purchases = purchases.Sum(p => p.TotalAmount);

            // المصاريف من الصندوق حسب الفئة
            var expenses = await _context.CashTransactions
                .Where(t => t.TransactionDate >= dateFrom && t.TransactionDate < endDate
                         && t.Type == TransactionType.Expense
                         && !t.IsCancelled
                         && t.Category != TransactionCategory.Purchase
                         && t.Category != TransactionCategory.SupplierPayment)
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.Amount) })
                .ToListAsync();

            foreach (var expense in expenses)
            {
                switch (expense.Category)
                {
                    case TransactionCategory.Electricity:
                        model.Electricity = expense.Amount;
                        break;
                    case TransactionCategory.Water:
                        model.Water = expense.Amount;
                        break;
                    case TransactionCategory.Communications:
                        model.Communications = expense.Amount;
                        break;
                    case TransactionCategory.Rent:
                        model.Rent = expense.Amount;
                        break;
                    case TransactionCategory.Salary:
                        model.Salaries = expense.Amount;
                        break;
                    case TransactionCategory.Transportation:
                        model.Transportation = expense.Amount;
                        break;
                    case TransactionCategory.Maintenance:
                        model.Maintenance = expense.Amount;
                        break;
                    case TransactionCategory.Tax:
                        model.Taxes = expense.Amount;
                        break;
                    case TransactionCategory.OtherExpense:
                        model.OtherExpenses += expense.Amount;
                        break;
                }
            }

            // ─── المقارنة مع الفترة السابقة ───
            var daysDiff = (dateTo - dateFrom).Days;
            var previousFrom = dateFrom.AddDays(-daysDiff - 1);
            var previousTo = dateFrom.AddDays(-1);
            var previousEndDate = previousTo.AddDays(1);

            var previousSales = await _context.Sales
                .Where(s => s.SaleDate >= previousFrom && s.SaleDate < previousEndDate
                         && s.Status != InvoiceStatus.Cancelled)
                .ToListAsync();

            var previousExpenses = await _context.CashTransactions
                .Where(t => t.TransactionDate >= previousFrom && t.TransactionDate < previousEndDate
                         && t.Type == TransactionType.Expense
                         && !t.IsCancelled
                         && t.Category != TransactionCategory.Purchase)
                .SumAsync(t => t.Amount);

            var previousRevenue = previousSales.Sum(s => s.TotalAmount);
            var previousCost = previousSales.Sum(s => s.TotalCost);
            model.PreviousNetProfit = previousRevenue - previousCost - previousExpenses;

            // ─── البيانات اليومية ───
            var allDates = new List<DateTime>();
            for (var date = dateFrom; date <= dateTo; date = date.AddDays(1))
            {
                allDates.Add(date.Date);
            }

            foreach (var date in allDates)
            {
                var nextDate = date.AddDays(1);

                var daySales = sales.Where(s => s.SaleDate.Date == date).ToList();
                var dayExpensesAmount = await _context.CashTransactions
                    .Where(t => t.TransactionDate >= date && t.TransactionDate < nextDate
                             && t.Type == TransactionType.Expense
                             && !t.IsCancelled)
                    .SumAsync(t => t.Amount);

                var dayRevenue = daySales.Sum(s => s.TotalAmount);
                var dayCost = daySales.Sum(s => s.TotalCost);

                model.DailyData.Add(new DailyProfitLossData
                {
                    Date = date,
                    Revenue = dayRevenue,
                    Expenses = dayCost + dayExpensesAmount
                });
            }

            // ─── تفصيل المصاريف ───
            var totalExpensesAmount = model.TotalExpenses;

            var expenseCategories = new List<(string Name, decimal Amount, string Color, string Icon)>
            {
                ("تكلفة البضاعة المباعة", model.CostOfGoodsSold, "#f5576c", "fa-box"),
                ("⚡ الكهرباء", model.Electricity, "#feca57", "fa-bolt"),
                ("💧 الماء", model.Water, "#4facfe", "fa-droplet"),
                ("📞 الاتصالات", model.Communications, "#a29bfe", "fa-phone"),
                ("🏠 الإيجار", model.Rent, "#fd79a8", "fa-house"),
                ("👔 الرواتب", model.Salaries, "#fdcb6e", "fa-user-tie"),
                ("🚚 النقل", model.Transportation, "#6c5ce7", "fa-truck"),
                ("🔧 الصيانة", model.Maintenance, "#00b894", "fa-wrench"),
                ("📄 الضرائب", model.Taxes, "#e17055", "fa-file-invoice-dollar"),
                ("➕ أخرى", model.OtherExpenses, "#636e72", "fa-receipt")
            };

            model.ExpenseBreakdown = expenseCategories
                .Where(e => e.Amount > 0)
                .Select(e => new ExpenseBreakdownData
                {
                    Category = e.Name,
                    Amount = e.Amount,
                    Color = e.Color,
                    Icon = e.Icon,
                    Percentage = totalExpensesAmount > 0
                        ? Math.Round((e.Amount / totalExpensesAmount) * 100, 2)
                        : 0
                })
                .OrderByDescending(e => e.Amount)
                .ToList();

            return model;
        }

        // ═══════════════════════════════════════════════════
        // 📦 تقرير المخزون الشامل
        // ═══════════════════════════════════════════════════

        public async Task<InventoryReportViewModel> GetInventoryReportAsync(
            int? categoryId = null,
            string stockFilter = "all")
        {
            var model = new InventoryReportViewModel
            {
                CategoryId = categoryId,
                StockFilter = stockFilter
            };

            var today = DateTime.Today;
            var expiringSoonDate = today.AddDays(30);
            var thirtyDaysAgo = today.AddDays(-30);

            // الفلتر الأساسي
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            // فلتر المخزون
            switch (stockFilter)
            {
                case "low":
                    query = query.Where(p => p.StockQuantity > 0 && p.StockQuantity <= p.MinStockAlert);
                    break;
                case "out":
                    query = query.Where(p => p.StockQuantity == 0);
                    break;
                case "expiring":
                    query = query.Where(p => p.ExpiryDate.HasValue
                                          && p.ExpiryDate >= today
                                          && p.ExpiryDate <= expiringSoonDate);
                    break;
                case "expired":
                    query = query.Where(p => p.ExpiryDate.HasValue && p.ExpiryDate < today);
                    break;
            }

            var products = await query.ToListAsync();

            // الملخص
            model.TotalProducts = await _context.Products.CountAsync();
            model.ActiveProducts = await _context.Products.CountAsync(p => p.IsActive);
            model.OutOfStockProducts = await _context.Products
                .CountAsync(p => p.IsActive && p.StockQuantity == 0);
            model.LowStockProducts = await _context.Products
                .CountAsync(p => p.IsActive && p.StockQuantity > 0 && p.StockQuantity <= p.MinStockAlert);
            model.ExpiringProducts = await _context.Products
                .CountAsync(p => p.IsActive && p.ExpiryDate.HasValue
                              && p.ExpiryDate >= today
                              && p.ExpiryDate <= expiringSoonDate);
            model.ExpiredProducts = await _context.Products
                .CountAsync(p => p.IsActive && p.ExpiryDate.HasValue && p.ExpiryDate < today);

            // قيم المخزون
            var allProducts = await _context.Products.Where(p => p.IsActive).ToListAsync();
            model.TotalInventoryValueAtPurchase = allProducts.Sum(p => p.StockQuantity * p.PurchasePriceTTC);
            model.TotalInventoryValueAtRetail = allProducts.Sum(p => p.StockQuantity * p.RetailPriceTTC);
            model.TotalInventoryValue = model.TotalInventoryValueAtPurchase;
            model.PotentialProfit = model.TotalInventoryValueAtRetail - model.TotalInventoryValueAtPurchase;

            // التوزيع حسب الفئة
            var categoryData = allProducts
                .Where(p => p.Category != null)
                .GroupBy(p => new { p.CategoryId, p.Category!.Name })
                .Select(g => new
                {
                    CategoryId = g.Key.CategoryId,
                    Name = g.Key.Name,
                    Count = g.Count(),
                    Quantity = g.Sum(p => p.StockQuantity),
                    Value = g.Sum(p => p.StockQuantity * p.PurchasePriceTTC)
                })
                .OrderByDescending(c => c.Value)
                .ToList();

            var colors = new[] { "#667eea", "#f5576c", "#11998e", "#fa709a", "#4facfe",
                                 "#43e97b", "#f093fb", "#feca57", "#48dbfb", "#ff6b6b" };

            model.CategoryDistribution = categoryData.Select((c, i) => new CategoryInventoryData
            {
                CategoryId = c.CategoryId,
                CategoryName = c.Name,
                ProductsCount = c.Count,
                TotalQuantity = c.Quantity,
                TotalValue = c.Value,
                Percentage = model.TotalInventoryValueAtPurchase > 0
                    ? Math.Round((c.Value / model.TotalInventoryValueAtPurchase) * 100, 2)
                    : 0,
                Color = colors[i % colors.Length]
            }).ToList();

            // الحصول على عدد المبيعات لكل منتج (آخر 30 يوم)
            var productSales = await _context.SaleItems
                .Include(si => si.Sale)
                .Where(si => si.Sale!.SaleDate >= thirtyDaysAgo
                          && si.Sale.Status != InvoiceStatus.Cancelled)
                .GroupBy(si => si.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(si => si.Quantity) })
                .ToListAsync();

            var salesDict = productSales.ToDictionary(s => s.ProductId, s => s.Quantity);

            // تحويل المنتجات إلى ProductInventoryData
            model.Products = products.Select(p => new ProductInventoryData
            {
                ProductId = p.Id,
                Name = p.Name,
                Reference = p.Reference,
                CategoryName = p.Category?.Name ?? "غير محدد",
                StockQuantity = p.StockQuantity,
                MinStockAlert = p.MinStockAlert,
                PurchasePrice = p.PurchasePriceTTC,
                RetailPrice = p.RetailPriceTTC,
                TotalValueAtCost = p.StockQuantity * p.PurchasePriceTTC,
                TotalValueAtRetail = p.StockQuantity * p.RetailPriceTTC,
                PotentialProfit = (p.StockQuantity * p.RetailPriceTTC) - (p.StockQuantity * p.PurchasePriceTTC),
                ExpiryDate = p.ExpiryDate,
                StockStatus = DetermineStockStatus(p, today, expiringSoonDate),
                QuantitySoldLast30Days = salesDict.ContainsKey(p.Id) ? salesDict[p.Id] : 0
            }).ToList();

            // الأعلى قيمة
            model.HighestValueProducts = model.Products
                .OrderByDescending(p => p.TotalValueAtCost)
                .Take(10)
                .ToList();

            // الأكثر حركة
            model.MostMovingProducts = model.Products
                .Where(p => p.QuantitySoldLast30Days > 0)
                .OrderByDescending(p => p.QuantitySoldLast30Days)
                .Take(10)
                .ToList();

            // الأقل حركة
            model.SlowestMovingProducts = model.Products
                .Where(p => p.StockQuantity > 0)
                .OrderBy(p => p.QuantitySoldLast30Days)
                .Take(10)
                .ToList();

            // Dropdowns
            model.Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return model;
        }


        // ═══════════════════════════════════════════════════
        // 👥 تقرير الشركاء (العملاء/الموردين)
        // ═══════════════════════════════════════════════════

        public async Task<PartnersReportViewModel> GetPartnersReportAsync(
            string reportType = "customers",
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            var model = new PartnersReportViewModel
            {
                ReportType = reportType,
                DateFrom = dateFrom ?? DateTime.Today.AddDays(-90),
                DateTo = dateTo ?? DateTime.Today
            };

            var endDate = model.DateTo.Date.AddDays(1);
            var today = DateTime.Today;

            // ─── ملخص العملاء ───
            model.TotalCustomers = await _context.Customers.CountAsync();
            model.ActiveCustomers = await _context.Customers.CountAsync(c => c.IsActive);

            // ─── ملخص الموردين ───
            model.TotalSuppliers = await _context.Suppliers.CountAsync();
            model.ActiveSuppliers = await _context.Suppliers.CountAsync(s => s.IsActive);

            // ═══════════════════════════════════════════════════
            // 👤 العملاء
            // ═══════════════════════════════════════════════════
            if (reportType == "customers" || reportType == "all")
            {
                var customers = await _context.Customers
                    .Where(c => c.IsActive)
                    .ToListAsync();

                var customerSalesData = new List<CustomerReportData>();

                foreach (var customer in customers)
                {
                    // فواتير العميل
                    var customerSales = await _context.Sales
                        .Where(s => s.CustomerId == customer.Id
                                 && s.Status != InvoiceStatus.Cancelled)
                        .ToListAsync();

                    // دفعات العميل
                    var customerPayments = await _context.CustomerPayments
                        .Where(cp => cp.CustomerId == customer.Id)
                        .ToListAsync();

                    var totalPurchases = customerSales.Sum(s => s.TotalAmount);
                    var totalPaid = customerPayments.Sum(cp => cp.Amount);
                    var currentDebt = customerSales.Sum(s => s.RemainingAmount);
                    var lastPurchaseDate = customerSales.Any()
                        ? customerSales.Max(s => s.SaleDate)
                        : (DateTime?)null;

                    customerSalesData.Add(new CustomerReportData
                    {
                        CustomerId = customer.Id,
                        Name = customer.Name,
                        Phone = customer.Phone,
                        City = customer.City,
                        InvoicesCount = customerSales.Count,
                        TotalPurchases = totalPurchases,
                        TotalPaid = totalPaid,
                        CurrentDebt = currentDebt,
                        LastPurchaseDate = lastPurchaseDate,
                        DaysSinceLastPurchase = lastPurchaseDate.HasValue
                            ? (int)(today - lastPurchaseDate.Value).TotalDays
                            : 0
                    });
                }

                model.Customers = customerSalesData
                    .Where(c => c.TotalPurchases > 0 || c.CurrentDebt > 0)
                    .OrderByDescending(c => c.CurrentDebt)
                    .ThenByDescending(c => c.TotalPurchases)
                    .ToList();

                // إجماليات العملاء
                model.TotalCustomersDebt = model.Customers.Sum(c => c.CurrentDebt);
                model.CustomersWithDebt = model.Customers.Count(c => c.CurrentDebt > 0);

                // أعمار الديون للعملاء (Aging)
                model.CustomerDebtAging = await CalculateCustomerDebtAgingAsync(today);
            }

            // ═══════════════════════════════════════════════════
            // 🚚 الموردين
            // ═══════════════════════════════════════════════════
            if (reportType == "suppliers" || reportType == "all")
            {
                var suppliers = await _context.Suppliers
                    .Where(s => s.IsActive)
                    .ToListAsync();

                var supplierPurchasesData = new List<SupplierReportData>();

                foreach (var supplier in suppliers)
                {
                    // فواتير المورد
                    var supplierPurchases = await _context.Purchases
                        .Where(p => p.SupplierId == supplier.Id
                                 && p.Status != InvoiceStatus.Cancelled)
                        .ToListAsync();

                    // دفعات للمورد
                    var supplierPayments = await _context.SupplierPayments
                        .Where(sp => sp.SupplierId == supplier.Id)
                        .ToListAsync();

                    var totalPurchases = supplierPurchases.Sum(p => p.TotalAmount);
                    var totalPaid = supplierPayments.Sum(sp => sp.Amount);
                    var currentDebt = supplierPurchases.Sum(p => p.RemainingAmount);
                    var lastPurchaseDate = supplierPurchases.Any()
                        ? supplierPurchases.Max(p => p.PurchaseDate)
                        : (DateTime?)null;

                    supplierPurchasesData.Add(new SupplierReportData
                    {
                        SupplierId = supplier.Id,
                        Name = supplier.Name,
                        Phone = supplier.Phone,
                        City = supplier.City,
                        InvoicesCount = supplierPurchases.Count,
                        TotalPurchases = totalPurchases,
                        TotalPaid = totalPaid,
                        CurrentDebt = currentDebt,
                        LastPurchaseDate = lastPurchaseDate
                    });
                }

                model.Suppliers = supplierPurchasesData
                    .Where(s => s.TotalPurchases > 0 || s.CurrentDebt > 0)
                    .OrderByDescending(s => s.CurrentDebt)
                    .ThenByDescending(s => s.TotalPurchases)
                    .ToList();

                // إجماليات الموردين
                model.TotalSuppliersDebt = model.Suppliers.Sum(s => s.CurrentDebt);
                model.SuppliersWithDebt = model.Suppliers.Count(s => s.CurrentDebt > 0);

                // أعمار الديون للموردين (Aging)
                model.SupplierDebtAging = await CalculateSupplierDebtAgingAsync(today);
            }

            return model;
        }

        // ═══════════════════════════════════════════════════
        // 💵 تقرير الصندوق الشامل
        // ═══════════════════════════════════════════════════

        public async Task<CashReportFullViewModel> GetCashReportAsync(
            DateTime dateFrom,
            DateTime dateTo,
            int? cashRegisterId = null)
        {
            var endDate = dateTo.Date.AddDays(1);
            var model = new CashReportFullViewModel
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                CashRegisterId = cashRegisterId,
                PeriodLabel = $"من {dateFrom:yyyy/MM/dd} إلى {dateTo:yyyy/MM/dd}"
            };

            // الفلتر الأساسي
            var query = _context.CashTransactions
                .Include(t => t.CashRegister)
                .Where(t => t.TransactionDate >= dateFrom
                         && t.TransactionDate < endDate
                         && !t.IsCancelled);

            if (cashRegisterId.HasValue)
                query = query.Where(t => t.CashRegisterId == cashRegisterId.Value);

            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            // ─── الملخص ───
            model.TotalIncome = transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            model.TotalExpense = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            model.TotalTransactions = transactions.Count;
            model.IncomeTransactions = transactions.Count(t => t.Type == TransactionType.Income);
            model.ExpenseTransactions = transactions.Count(t => t.Type == TransactionType.Expense);

            // الرصيد الافتتاحي (آخر حركة قبل الفترة)
            var lastBeforePeriod = await _context.CashTransactions
                .Where(t => t.TransactionDate < dateFrom && !t.IsCancelled)
                .Where(t => !cashRegisterId.HasValue || t.CashRegisterId == cashRegisterId.Value)
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.Id)
                .FirstOrDefaultAsync();

            model.OpeningBalance = lastBeforePeriod?.BalanceAfter ?? 0;
            model.ClosingBalance = transactions.Any()
                ? transactions.OrderByDescending(t => t.TransactionDate).First().BalanceAfter
                : model.OpeningBalance;

            // ─── التدفق النقدي اليومي ───
            var allDates = new List<DateTime>();
            for (var date = dateFrom; date <= dateTo; date = date.AddDays(1))
            {
                allDates.Add(date.Date);
            }

            decimal runningBalance = model.OpeningBalance;

            foreach (var date in allDates)
            {
                var dayTransactions = transactions.Where(t => t.TransactionDate.Date == date).ToList();

                var dayIncome = dayTransactions
                    .Where(t => t.Type == TransactionType.Income)
                    .Sum(t => t.Amount);

                var dayExpense = dayTransactions
                    .Where(t => t.Type == TransactionType.Expense)
                    .Sum(t => t.Amount);

                runningBalance += dayIncome - dayExpense;

                model.DailyCashFlow.Add(new DailyCashFlowData
                {
                    Date = date,
                    Income = dayIncome,
                    Expense = dayExpense,
                    Balance = runningBalance
                });
            }

            // ─── الإيرادات حسب الفئة ───
            var incomeByCategory = transactions
                .Where(t => t.Type == TransactionType.Income)
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Amount = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Amount)
                .ToList();

            model.IncomeByCategory = incomeByCategory.Select(g => new CategoryCashData
            {
                Category = g.Category,
                Amount = g.Amount,
                Count = g.Count,
                Percentage = model.TotalIncome > 0
                    ? Math.Round((g.Amount / model.TotalIncome) * 100, 2)
                    : 0
            }).ToList();

            // ─── المصاريف حسب الفئة ───
            var expenseByCategory = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Amount = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Amount)
                .ToList();

            model.ExpenseByCategory = expenseByCategory.Select(g => new CategoryCashData
            {
                Category = g.Category,
                Amount = g.Amount,
                Count = g.Count,
                Percentage = model.TotalExpense > 0
                    ? Math.Round((g.Amount / model.TotalExpense) * 100, 2)
                    : 0
            }).ToList();

            // ─── حسب طريقة الدفع ───
            var byPaymentMethod = transactions
                .GroupBy(t => t.PaymentMethod)
                .Select(g => new
                {
                    Method = g.Key,
                    Amount = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Amount)
                .ToList();

            var totalAmount = transactions.Sum(t => t.Amount);

            model.ByPaymentMethod = byPaymentMethod.Select(g => new PaymentMethodData
            {
                Method = g.Method,
                Amount = g.Amount,
                Count = g.Count,
                Percentage = totalAmount > 0
                    ? Math.Round((g.Amount / totalAmount) * 100, 2)
                    : 0
            }).ToList();

            // ─── الحركات التفصيلية ───
            model.Transactions = transactions;

            // ─── الجردات اليومية ───
            var closuresQuery = _context.DailyClosures
                .Include(c => c.CashRegister)
                .Where(c => c.ClosureDate >= dateFrom && c.ClosureDate <= dateTo);

            if (cashRegisterId.HasValue)
                closuresQuery = closuresQuery.Where(c => c.CashRegisterId == cashRegisterId.Value);

            model.Closures = await closuresQuery
                .OrderByDescending(c => c.ClosureDate)
                .ToListAsync();

            model.ClosuresCount = model.Closures.Count;
            model.TotalDifferences = model.Closures.Sum(c => Math.Abs(c.Difference));

            // ─── Dropdowns ───
            model.CashRegisters = await _context.CashRegisters
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return model;
        }

        // ═══════════════════════════════════════════════════
        // 🛠️ Helpers لأعمار الديون
        // ═══════════════════════════════════════════════════

        private async Task<List<DebtAgingData>> CalculateCustomerDebtAgingAsync(DateTime today)
        {
            var unpaidSales = await _context.Sales
                .Where(s => s.Status != InvoiceStatus.Cancelled && s.RemainingAmount > 0)
                .ToListAsync();

            var aging = new[]
            {
                new { Period = "0-30 يوم", MinDays = 0, MaxDays = 30, Color = "#11998e" },
                new { Period = "31-60 يوم", MinDays = 31, MaxDays = 60, Color = "#feca57" },
                new { Period = "61-90 يوم", MinDays = 61, MaxDays = 90, Color = "#fa709a" },
                new { Period = "أكثر من 90 يوم", MinDays = 91, MaxDays = 99999, Color = "#f5576c" }
            };

            return aging.Select(a =>
            {
                var matchingSales = unpaidSales.Where(s =>
                {
                    var daysOld = (today - s.SaleDate).Days;
                    return daysOld >= a.MinDays && daysOld <= a.MaxDays;
                }).ToList();

                return new DebtAgingData
                {
                    Period = a.Period,
                    Amount = matchingSales.Sum(s => s.RemainingAmount),
                    Count = matchingSales.Count,
                    Color = a.Color
                };
            }).ToList();
        }

        private async Task<List<DebtAgingData>> CalculateSupplierDebtAgingAsync(DateTime today)
        {
            var unpaidPurchases = await _context.Purchases
                .Where(p => p.Status != InvoiceStatus.Cancelled && p.RemainingAmount > 0)
                .ToListAsync();

            var aging = new[]
            {
                new { Period = "0-30 يوم", MinDays = 0, MaxDays = 30, Color = "#11998e" },
                new { Period = "31-60 يوم", MinDays = 31, MaxDays = 60, Color = "#feca57" },
                new { Period = "61-90 يوم", MinDays = 61, MaxDays = 90, Color = "#fa709a" },
                new { Period = "أكثر من 90 يوم", MinDays = 91, MaxDays = 99999, Color = "#f5576c" }
            };

            return aging.Select(a =>
            {
                var matchingPurchases = unpaidPurchases.Where(p =>
                {
                    var daysOld = (today - p.PurchaseDate).Days;
                    return daysOld >= a.MinDays && daysOld <= a.MaxDays;
                }).ToList();

                return new DebtAgingData
                {
                    Period = a.Period,
                    Amount = matchingPurchases.Sum(p => p.RemainingAmount),
                    Count = matchingPurchases.Count,
                    Color = a.Color
                };
            }).ToList();
        }

        // ═══════════════════════════════════════════════════
        // 🛠️ Helpers
        // ═══════════════════════════════════════════════════

        private string DetermineStockStatus(Product product, DateTime today, DateTime expiringSoonDate)
        {
            if (product.StockQuantity == 0) return "out";
            if (product.ExpiryDate.HasValue && product.ExpiryDate < today) return "expired";
            if (product.ExpiryDate.HasValue && product.ExpiryDate <= expiringSoonDate) return "expiring";
            if (product.StockQuantity <= product.MinStockAlert) return "low";
            return "ok";
        }
    }
}