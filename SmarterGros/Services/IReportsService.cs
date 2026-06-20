using SmarterGros.ViewModels.Reports;

namespace SmarterGros.Services
{
    /// <summary>
    /// 📊 Interface لخدمة التقارير الشاملة
    /// </summary>
    public interface IReportsService
    {
        // الصفحة الرئيسية
        Task<ReportsListViewModel> GetReportsListAsync();

        // تقرير المبيعات
        Task<SalesReportViewModel> GetSalesReportAsync(
            DateTime dateFrom,
            DateTime dateTo,
            int? customerId = null,
            int? categoryId = null);

        // تقرير المشتريات
        Task<PurchasesReportViewModel> GetPurchasesReportAsync(
            DateTime dateFrom,
            DateTime dateTo,
            int? supplierId = null,
            int? categoryId = null);

        // تقرير الأرباح والخسائر
        Task<ProfitLossReportViewModel> GetProfitLossReportAsync(
            DateTime dateFrom,
            DateTime dateTo);

        // تقرير المخزون
        Task<InventoryReportViewModel> GetInventoryReportAsync(
            int? categoryId = null,
            string stockFilter = "all");

        // تقرير الشركاء (العملاء/الموردين)
        Task<PartnersReportViewModel> GetPartnersReportAsync(
            string reportType = "customers",
            DateTime? dateFrom = null,
            DateTime? dateTo = null);

        // تقرير الصندوق
        Task<CashReportFullViewModel> GetCashReportAsync(
            DateTime dateFrom,
            DateTime dateTo,
            int? cashRegisterId = null);
    }
}