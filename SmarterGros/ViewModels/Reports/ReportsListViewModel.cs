namespace SmarterGros.ViewModels.Reports
{
    /// <summary>
    /// 📊 ViewModel لصفحة قائمة التقارير الرئيسية
    /// </summary>
    public class ReportsListViewModel
    {
        // ملخص سريع
        public decimal TotalSalesMonth { get; set; }
        public decimal TotalPurchasesMonth { get; set; }
        public decimal TotalProfitMonth { get; set; }
        public int TotalInvoicesMonth { get; set; }
        public decimal CashBalance { get; set; }
        public int LowStockCount { get; set; }

        // قائمة التقارير المتاحة
        public List<ReportCardInfo> AvailableReports { get; set; } = new();
    }

    /// <summary>
    /// 📋 معلومات كل تقرير في الصفحة الرئيسية
    /// </summary>
    public class ReportCardInfo
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "fa-chart-bar";
        public string Color { get; set; } = "primary";
        public string ActionName { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}