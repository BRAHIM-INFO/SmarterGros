using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmarterGros.Security;
using SmarterGros.Services;

namespace SmarterGros.Controllers
{
    /// <summary>
    /// 📊 Controller التقارير الشاملة
    /// </summary>
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IReportsService _reportsService;
        private readonly IActivityLogService _activityLogService;

        public ReportsController(
            IReportsService reportsService,
            IActivityLogService activityLogService)
        {
            _reportsService = reportsService;
            _activityLogService = activityLogService;
        }

        // ═══════════════════════════════════════════════════
        // 🏠 الصفحة الرئيسية للتقارير
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Reports.ViewStatistics)]
        public async Task<IActionResult> Index()
        {
            var model = await _reportsService.GetReportsListAsync();

            await _activityLogService.LogViewAsync(
                module: "Reports",
                description: "عرض صفحة التقارير الرئيسية");

            return View(model);
        }

        // ═══════════════════════════════════════════════════
        // 📊 صفحة الإحصائيات (المحسّنة)
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Reports.ViewStatistics)]
        public async Task<IActionResult> Statistics()
        {
            // نستخدم تقرير الأرباح والخسائر للشهر الحالي
            var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var today = DateTime.Today;

            var model = await _reportsService.GetProfitLossReportAsync(monthStart, today);

            await _activityLogService.LogViewAsync(
                module: "Reports",
                description: "عرض الإحصائيات الشهرية");

            return View(model);
        }

        // ═══════════════════════════════════════════════════
        // 💰 تقرير المبيعات
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Reports.ViewSalesReport)]
        public async Task<IActionResult> SalesReport(
            DateTime? dateFrom,
            DateTime? dateTo,
            int? customerId,
            int? categoryId)
        {
            // افتراضي: آخر 30 يوم
            var from = dateFrom ?? DateTime.Today.AddDays(-30);
            var to = dateTo ?? DateTime.Today;

            var model = await _reportsService.GetSalesReportAsync(from, to, customerId, categoryId);

            await _activityLogService.LogViewAsync(
                module: "Reports",
                description: $"عرض تقرير المبيعات من {from:yyyy/MM/dd} إلى {to:yyyy/MM/dd}");

            return View(model);
        }

        // ═══════════════════════════════════════════════════
        // 🛒 تقرير المشتريات
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Reports.ViewPurchasesReport)]
        public async Task<IActionResult> PurchasesReport(
            DateTime? dateFrom,
            DateTime? dateTo,
            int? supplierId,
            int? categoryId)
        {
            var from = dateFrom ?? DateTime.Today.AddDays(-30);
            var to = dateTo ?? DateTime.Today;

            var model = await _reportsService.GetPurchasesReportAsync(from, to, supplierId, categoryId);

            await _activityLogService.LogViewAsync(
                module: "Reports",
                description: $"عرض تقرير المشتريات من {from:yyyy/MM/dd} إلى {to:yyyy/MM/dd}");

            return View(model);
        }

        // ═══════════════════════════════════════════════════
        // 💰 تقرير الأرباح والخسائر
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Reports.ViewProfitReport)]
        public async Task<IActionResult> ProfitLossReport(
            DateTime? dateFrom,
            DateTime? dateTo)
        {
            var from = dateFrom ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var to = dateTo ?? DateTime.Today;

            var model = await _reportsService.GetProfitLossReportAsync(from, to);

            await _activityLogService.LogViewAsync(
                module: "Reports",
                description: $"عرض تقرير الأرباح والخسائر من {from:yyyy/MM/dd} إلى {to:yyyy/MM/dd}");

            return View(model);
        }

        // ═══════════════════════════════════════════════════
        // 📦 تقرير المخزون
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Reports.ViewStockReport)]
        public async Task<IActionResult> InventoryReport(
            int? categoryId,
            string stockFilter = "all")
        {
            var model = await _reportsService.GetInventoryReportAsync(categoryId, stockFilter);

            await _activityLogService.LogViewAsync(
                module: "Reports",
                description: $"عرض تقرير المخزون (فلتر: {stockFilter})");

            return View(model);
        }

        // ═══════════════════════════════════════════════════
        // 👥 تقرير الشركاء (العملاء/الموردين)
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Reports.ViewStatistics)]
        public async Task<IActionResult> PartnersReport(
            string reportType = "customers",
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            var model = await _reportsService.GetPartnersReportAsync(reportType, dateFrom, dateTo);

            await _activityLogService.LogViewAsync(
                module: "Reports",
                description: $"عرض تقرير {(reportType == "customers" ? "العملاء" : "الموردين")}");

            return View(model);
        }

        // ═══════════════════════════════════════════════════
        // 💵 تقرير الصندوق
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Reports.ViewStatistics)]
        public async Task<IActionResult> CashReport(
            DateTime? dateFrom,
            DateTime? dateTo,
            int? cashRegisterId)
        {
            var from = dateFrom ?? DateTime.Today.AddDays(-30);
            var to = dateTo ?? DateTime.Today;

            var model = await _reportsService.GetCashReportAsync(from, to, cashRegisterId);

            await _activityLogService.LogViewAsync(
                module: "Reports",
                description: $"عرض تقرير الصندوق من {from:yyyy/MM/dd} إلى {to:yyyy/MM/dd}");

            return View(model);
        }

        // ═══════════════════════════════════════════════════
        // 🖨️ نسخ الطباعة (سننشئها لاحقاً)
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Reports.ViewSalesReport)]
        public async Task<IActionResult> PrintSalesReport(
            DateTime dateFrom,
            DateTime dateTo,
            int? customerId,
            int? categoryId)
        {
            var model = await _reportsService.GetSalesReportAsync(dateFrom, dateTo, customerId, categoryId);

            await _activityLogService.LogAsync(
                actionType: "Print",
                actionName: "طباعة تقرير المبيعات",
                module: "Reports",
                description: $"طباعة تقرير المبيعات من {dateFrom:yyyy/MM/dd} إلى {dateTo:yyyy/MM/dd}");

            return View("Print/PrintSalesReport", model);
        }

        [HttpGet]
        [HasPermission(Permissions.Reports.ViewPurchasesReport)]
        public async Task<IActionResult> PrintPurchasesReport(
            DateTime dateFrom,
            DateTime dateTo,
            int? supplierId,
            int? categoryId)
        {
            var model = await _reportsService.GetPurchasesReportAsync(dateFrom, dateTo, supplierId, categoryId);

            await _activityLogService.LogAsync(
                actionType: "Print",
                actionName: "طباعة تقرير المشتريات",
                module: "Reports");

            return View("Print/PrintPurchasesReport", model);
        }

        [HttpGet]
        [HasPermission(Permissions.Reports.ViewProfitReport)]
        public async Task<IActionResult> PrintProfitLossReport(
            DateTime dateFrom,
            DateTime dateTo)
        {
            var model = await _reportsService.GetProfitLossReportAsync(dateFrom, dateTo);

            await _activityLogService.LogAsync(
                actionType: "Print",
                actionName: "طباعة تقرير الأرباح والخسائر",
                module: "Reports");

            return View("Print/PrintProfitLossReport", model);
        }

        [HttpGet]
        [HasPermission(Permissions.Reports.ViewStockReport)]
        public async Task<IActionResult> PrintInventoryReport(
            int? categoryId,
            string stockFilter = "all")
        {
            var model = await _reportsService.GetInventoryReportAsync(categoryId, stockFilter);

            await _activityLogService.LogAsync(
                actionType: "Print",
                actionName: "طباعة تقرير المخزون",
                module: "Reports");

            return View("Print/PrintInventoryReport", model);
        }

        // ═══════════════════════════════════════════════════
        // 📥 التصدير (Excel/PDF) - placeholder
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Reports.ExportToExcel)]
        public async Task<IActionResult> ExportSalesReportExcel(
            DateTime dateFrom,
            DateTime dateTo,
            int? customerId,
            int? categoryId)
        {
            await _activityLogService.LogAsync(
                actionType: "Export",
                actionName: "تصدير تقرير المبيعات Excel",
                module: "Reports");

            TempData["Info"] = "ميزة التصدير قيد التطوير - استخدم زر الطباعة وحفظها كـ PDF";
            return RedirectToAction(nameof(SalesReport), new { dateFrom, dateTo, customerId, categoryId });
        }
    }
}