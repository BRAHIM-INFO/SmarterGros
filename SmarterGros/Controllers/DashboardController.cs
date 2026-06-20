using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmarterGros.Security;
using SmarterGros.Services;

namespace SmarterGros.Controllers
{
    /// <summary>
    /// 📊 Controller لوحة التحكم
    /// </summary>
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        [HasPermission(Permissions.Dashboard.View)]
        public async Task<IActionResult> Index()
        {
            var model = await _dashboardService.GetDashboardDataAsync();
            return View(model);
        }

        /// <summary>
        /// API لتحديث البيانات تلقائياً
        /// </summary>
        [HttpGet]
        [HasPermission(Permissions.Dashboard.View)]
        public async Task<IActionResult> GetDashboardData()
        {
            var model = await _dashboardService.GetDashboardDataAsync();
            return Json(model);
        }
    }
}