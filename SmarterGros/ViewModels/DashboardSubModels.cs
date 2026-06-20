namespace SmarterGros.ViewModels
{
    /// <summary>
    /// 💰 الأداء المالي لفترة معينة
    /// </summary>
    public class FinancialPerformance
    {
        public decimal SalesAmount { get; set; }
        public decimal PurchasesAmount { get; set; }
        public decimal ExpensesAmount { get; set; }
        public decimal CollectedAmount { get; set; }
        public decimal NetProfit { get; set; }
        public int SalesCount { get; set; }
        public int PurchasesCount { get; set; }
    }

    /// <summary>
    /// 📊 بطاقة KPI
    /// </summary>
    public class KpiCard
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = "0";
        public string Subtitle { get; set; } = string.Empty;
        public string Icon { get; set; } = "fa-chart-bar";
        public string Color { get; set; } = "primary";
        public decimal ChangePercentage { get; set; } = 0;
        public bool IsPositive { get; set; } = true;
        public string? ActionUrl { get; set; }
    }

    /// <summary>
    /// 📅 بيانات يومية للرسم البياني
    /// </summary>
    public class DailyTransactionData
    {
        public DateTime Date { get; set; }
        public string DateLabel => Date.ToString("dd/MM");
        public string DayName => Date.ToString("dddd", new System.Globalization.CultureInfo("ar-DZ"));
        public decimal SalesAmount { get; set; }
        public decimal PurchasesAmount { get; set; }
        public int SalesCount { get; set; }
        public int PurchasesCount { get; set; }
    }

    /// <summary>
    /// 💰 بيانات يومية للأرباح
    /// </summary>
    public class DailyProfitData
    {
        public DateTime Date { get; set; }
        public string DateLabel => Date.ToString("dd/MM");
        public decimal Profit { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
    }

    /// <summary>
    /// 🥧 توزيع حسب الفئة (Pie Chart)
    /// </summary>
    public class CategoryDistribution
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public string Color { get; set; } = "#0E4D3A";
    }

    /// <summary>
    /// 💸 تدفق نقدي
    /// </summary>
    public class CashFlowData
    {
        public DateTime Date { get; set; }
        public string DateLabel => Date.ToString("dd/MM");
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal Balance { get; set; }
    }

    /// <summary>
    /// 🏆 أعلى منتج مبيعاً
    /// </summary>
    public class TopProduct
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Reference { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalProfit { get; set; }
        public int CurrentStock { get; set; }
    }

    /// <summary>
    /// 👤 أعلى عميل
    /// </summary>
    public class TopCustomer
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalPurchases { get; set; }
        public int InvoicesCount { get; set; }
        public decimal CurrentDebt { get; set; }
    }

    /// <summary>
    /// 🚚 أعلى مورد
    /// </summary>
    public class TopSupplier
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalPurchases { get; set; }
        public int InvoicesCount { get; set; }
        public decimal CurrentDebt { get; set; }
    }

    /// <summary>
    /// 📋 فاتورة حديثة
    /// </summary>
    public class RecentInvoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty; // عميل أو مورد
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusColor { get; set; } = "secondary";
        public string PaymentType { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>
    /// 💵 حركة صندوق حديثة
    /// </summary>
    public class RecentCashTransaction
    {
        public int Id { get; set; }
        public string TransactionNumber { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsIncome { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Icon { get; set; } = "fa-money-bill";
    }

    /// <summary>
    /// ⚠️ تنبيه
    /// </summary>
    public class DashboardAlert
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Icon { get; set; } = "fa-exclamation-triangle";
        public string Color { get; set; } = "warning"; // success, info, warning, danger
        public string? ActionUrl { get; set; }
        public string? ActionText { get; set; }
        public int Priority { get; set; } = 1; // 1=high, 2=medium, 3=low
    }
}