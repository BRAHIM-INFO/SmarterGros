using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmarterGros.Models;
using SmarterGros.Security;
using SmarterGros.Services;
using SmarterGros.ViewModels;

namespace SmarterGros.Controllers
{
    [Authorize]
    public class ActivityLogsController : Controller
    {
        private readonly IActivityLogService _activityLogService;

        public ActivityLogsController(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
        }

        // ═══════════════════════════════════════════════════
        // 📝 عرض السجل الكامل
        // ═══════════════════════════════════════════════════
        [HasPermission(Permissions.ActivityLogs.View)]
        public async Task<IActionResult> Index(
            string? search,
            string? userId,
            string? actionType,
            string? module,
            string? severity,
            DateTime? dateFrom,
            DateTime? dateTo,
            int page = 1)
        {
            const int pageSize = 50;

            // الحصول على السجلات
            var logs = await _activityLogService.GetLogsAsync(
                search, userId, actionType, module, severity,
                dateFrom, dateTo, page, pageSize);

            // عدد السجلات الكلي للـ Pagination
            var totalCount = await _activityLogService.GetLogsCountAsync(
                search, userId, actionType, module, severity, dateFrom, dateTo);

            // الحصول على البيانات للفلاتر
            var users = await _activityLogService.GetActiveUsersAsync();
            var modules = await _activityLogService.GetActiveModulesAsync();

            // الإحصائيات
            var totalLogs = await _activityLogService.GetLogsCountAsync();
            var todayLogs = await _activityLogService.GetTodayLogsCountAsync();
            var criticalLogs = await _activityLogService.GetCriticalLogsCountAsync();
            var failedLogs = await _activityLogService.GetFailedLogsCountAsync();

            // أنواع العمليات (ثوابت)
            var actionTypes = new List<string>
            {
                ActivityActionTypes.Create,
                ActivityActionTypes.Update,
                ActivityActionTypes.Delete,
                ActivityActionTypes.View,
                ActivityActionTypes.Login,
                ActivityActionTypes.Logout,
                ActivityActionTypes.Export,
                ActivityActionTypes.Import,
                ActivityActionTypes.Print,
                ActivityActionTypes.Backup,
                ActivityActionTypes.Restore,
                ActivityActionTypes.Failed
            };

            var vm = new ActivityLogIndexViewModel
            {
                Logs = logs,
                Search = search,
                UserId = userId,
                ActionType = actionType,
                Module = module,
                Severity = severity,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Users = users,
                ActionTypes = actionTypes,
                Modules = modules,
                TotalLogs = totalLogs,
                TodayLogs = todayLogs,
                CriticalLogs = criticalLogs,
                FailedLogs = failedLogs,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return View(vm);
        }

        // ═══════════════════════════════════════════════════
        // 🔍 عرض تفاصيل سجل واحد
        // ═══════════════════════════════════════════════════
        [HttpGet]
        [HasPermission(Permissions.ActivityLogs.View)]
        public async Task<IActionResult> Details(int id)
        {
            var log = await _activityLogService.GetLogByIdAsync(id);
            if (log == null)
                return Json(new { success = false, message = "السجل غير موجود" });

            return Json(new
            {
                success = true,
                log = new
                {
                    log.Id,
                    log.UserName,
                    log.UserFullName,
                    log.UserRole,
                    log.ActionType,
                    log.ActionName,
                    actionTypeArabic = log.GetActionTypeArabic(),
                    actionIcon = log.GetActionIcon(),
                    actionColor = log.GetActionColor(),
                    log.Module,
                    log.EntityName,
                    log.EntityId,
                    log.Description,
                    log.OldValues,
                    log.NewValues,
                    log.IpAddress,
                    log.UserAgent,
                    log.RequestUrl,
                    log.RequestMethod,
                    log.IsSuccess,
                    log.ErrorMessage,
                    log.Severity,
                    severityArabic = log.GetSeverityArabic(),
                    severityColor = log.GetSeverityColor(),
                    createdAt = log.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    timeAgo = log.GetTimeAgo()
                }
            });
        }

        // ═══════════════════════════════════════════════════
        // 📜 تاريخ سجل معين (Entity History)
        // ═══════════════════════════════════════════════════
        [HttpGet]
        [HasPermission(Permissions.ActivityLogs.View)]
        public async Task<IActionResult> EntityHistory(string entityName, int entityId)
        {
            var history = await _activityLogService.GetEntityHistoryAsync(entityName, entityId, 50);

            return Json(new
            {
                success = true,
                history = history.Select(h => new
                {
                    h.Id,
                    h.UserFullName,
                    h.UserName,
                    actionTypeArabic = h.GetActionTypeArabic(),
                    actionIcon = h.GetActionIcon(),
                    actionColor = h.GetActionColor(),
                    h.Description,
                    timeAgo = h.GetTimeAgo(),
                    createdAt = h.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                })
            });
        }

        // ═══════════════════════════════════════════════════
        // 👤 نشاطات مستخدم معين
        // ═══════════════════════════════════════════════════
        [HttpGet]
        [HasPermission(Permissions.ActivityLogs.View)]
        public async Task<IActionResult> UserActivity(string userId, int count = 20)
        {
            var activities = await _activityLogService.GetUserRecentActivityAsync(userId, count);

            return Json(new
            {
                success = true,
                activities = activities.Select(a => new
                {
                    a.Id,
                    actionTypeArabic = a.GetActionTypeArabic(),
                    actionIcon = a.GetActionIcon(),
                    actionColor = a.GetActionColor(),
                    a.ActionName,
                    a.Module,
                    a.Description,
                    timeAgo = a.GetTimeAgo(),
                    createdAt = a.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                })
            });
        }

        // ═══════════════════════════════════════════════════
        // 🗑️ حذف سجل واحد
        // ═══════════════════════════════════════════════════
        [HttpPost]
        [HasPermission(Permissions.ActivityLogs.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _activityLogService.DeleteLogAsync(id);

            if (!result)
                return Json(new { success = false, message = "السجل غير موجود" });

            return Json(new { success = true, message = "تم حذف السجل بنجاح" });
        }

        // ═══════════════════════════════════════════════════
        // 🗑️ حذف السجلات القديمة
        // ═══════════════════════════════════════════════════
        [HttpPost]
        [HasPermission(Permissions.ActivityLogs.Delete)]
        public async Task<IActionResult> DeleteOldLogs(int daysOld)
        {
            if (daysOld < 1)
                return Json(new { success = false, message = "عدد الأيام يجب أن يكون أكبر من 0" });

            var beforeDate = DateTime.Now.AddDays(-daysOld);
            var deletedCount = await _activityLogService.DeleteOldLogsAsync(beforeDate);

            return Json(new
            {
                success = true,
                message = $"تم حذف {deletedCount} سجل أقدم من {daysOld} يوم",
                count = deletedCount
            });
        }

        // ═══════════════════════════════════════════════════
        // 🗑️ حذف كل السجلات (خطير!)
        // ═══════════════════════════════════════════════════
        [HttpPost]
        [HasPermission(Permissions.ActivityLogs.Delete)]
        public async Task<IActionResult> DeleteAll()
        {
            var deletedCount = await _activityLogService.DeleteAllLogsAsync();

            return Json(new
            {
                success = true,
                message = $"تم حذف جميع السجلات ({deletedCount} سجل)",
                count = deletedCount
            });
        }

        // ═══════════════════════════════════════════════════
        // 📊 إحصائيات سريعة (للـ Dashboard)
        // ═══════════════════════════════════════════════════
        [HttpGet]
        [HasPermission(Permissions.ActivityLogs.View)]
        public async Task<IActionResult> GetStats()
        {
            var totalLogs = await _activityLogService.GetLogsCountAsync();
            var todayLogs = await _activityLogService.GetTodayLogsCountAsync();
            var criticalLogs = await _activityLogService.GetCriticalLogsCountAsync();
            var failedLogs = await _activityLogService.GetFailedLogsCountAsync();

            return Json(new
            {
                success = true,
                stats = new
                {
                    total = totalLogs,
                    today = todayLogs,
                    critical = criticalLogs,
                    failed = failedLogs
                }
            });
        }
    }
}