namespace SmarterGros.ViewModels.Reports
{
    /// <summary>
    /// 💰 ViewModel لتقرير الأرباح والخسائر
    /// </summary>
    public class ProfitLossReportViewModel
    {
        public DateTime DateFrom { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime DateTo { get; set; } = DateTime.Today;
        public string PeriodLabel { get; set; } = string.Empty;

        // ═══════════════════════════════════════════════════
        // 📈 الإيرادات (Revenues)
        // ═══════════════════════════════════════════════════
        public decimal SalesRevenue { get; set; }
        public decimal CashCollections { get; set; }
        public decimal OtherIncome { get; set; }
        public decimal TotalRevenues => SalesRevenue + OtherIncome;

        // ═══════════════════════════════════════════════════
        // 📉 المصاريف (Expenses)
        // ═══════════════════════════════════════════════════
        public decimal CostOfGoodsSold { get; set; }  // تكلفة البضاعة المباعة
        public decimal Purchases { get; set; }
        public decimal Electricity { get; set; }
        public decimal Water { get; set; }
        public decimal Communications { get; set; }
        public decimal Rent { get; set; }
        public decimal Salaries { get; set; }
        public decimal Transportation { get; set; }
        public decimal Maintenance { get; set; }
        public decimal Taxes { get; set; }
        public decimal OtherExpenses { get; set; }
        public decimal TotalExpenses =>
            CostOfGoodsSold + Electricity + Water + Communications +
            Rent + Salaries + Transportation + Maintenance + Taxes + OtherExpenses;

        // ═══════════════════════════════════════════════════
        // 💰 الأرباح
        // ═══════════════════════════════════════════════════
        public decimal GrossProfit => SalesRevenue - CostOfGoodsSold;
        public decimal NetProfit => TotalRevenues - TotalExpenses;
        public decimal GrossProfitMargin =>
            SalesRevenue > 0 ? Math.Round((GrossProfit / SalesRevenue) * 100, 2) : 0;
        public decimal NetProfitMargin =>
            TotalRevenues > 0 ? Math.Round((NetProfit / TotalRevenues) * 100, 2) : 0;

        // ═══════════════════════════════════════════════════
        // 📊 المقارنة مع الفترة السابقة
        // ═══════════════════════════════════════════════════
        public decimal PreviousNetProfit { get; set; }
        public decimal ProfitGrowthPercent =>
            PreviousNetProfit > 0
                ? Math.Round(((NetProfit - PreviousNetProfit) / PreviousNetProfit) * 100, 2)
                : 0;

        // ═══════════════════════════════════════════════════
        // 📈 بيانات الرسوم البيانية
        // ═══════════════════════════════════════════════════
        public List<DailyProfitLossData> DailyData { get; set; } = new();
        public List<ExpenseBreakdownData> ExpenseBreakdown { get; set; } = new();
    }

    public class DailyProfitLossData
    {
        public DateTime Date { get; set; }
        public string DateLabel => Date.ToString("dd/MM");
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }
        public decimal NetProfit => Revenue - Expenses;
    }

    public class ExpenseBreakdownData
    {
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
        public string Color { get; set; } = "#667eea";
        public string Icon { get; set; } = "fa-receipt";
    }
}