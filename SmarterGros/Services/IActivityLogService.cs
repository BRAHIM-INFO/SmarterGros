using SmarterGros.Models;
using SmarterGros.ViewModels;

namespace SmarterGros.Services
{
    /// <summary>
    /// واجهة خدمة تسجيل النشاطات
    /// تُستخدم من أي مكان في النظام لتسجيل العمليات
    /// </summary>
    public interface IActivityLogService
    {
        // ═══════════════════════════════════════════════════
        // 📝 دوال التسجيل
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// تسجيل عملية عامة
        /// </summary>
        Task LogAsync(
            string actionType,
            string actionName,
            string? module = null,
            string? entityName = null,
            int? entityId = null,
            string? description = null,
            object? oldValues = null,
            object? newValues = null,
            string severity = "Info",
            bool isSuccess = true,
            string? errorMessage = null
        );

        /// <summary>
        /// تسجيل عملية إضافة (Create)
        /// </summary>
        Task LogCreateAsync(
            string module,
            string entityName,
            int entityId,
            string description,
            object? newValues = null
        );

        /// <summary>
        /// تسجيل عملية تعديل (Update)
        /// </summary>
        Task LogUpdateAsync(
            string module,
            string entityName,
            int entityId,
            string description,
            object? oldValues = null,
            object? newValues = null
        );

        /// <summary>
        /// تسجيل عملية حذف (Delete) - تُسجل كـ Critical
        /// </summary>
        Task LogDeleteAsync(
            string module,
            string entityName,
            int entityId,
            string description,
            object? deletedData = null
        );

        /// <summary>
        /// تسجيل عملية عرض (View)
        /// </summary>
        Task LogViewAsync(
            string module,
            string? description = null
        );

        /// <summary>
        /// تسجيل عملية تسجيل دخول (Login)
        /// </summary>
        Task LogLoginAsync(
            string userName,
            bool isSuccess,
            string? errorMessage = null
        );

        /// <summary>
        /// تسجيل عملية تسجيل خروج (Logout)
        /// </summary>
        Task LogLogoutAsync();

        /// <summary>
        /// تسجيل خطأ أو فشل عملية
        /// </summary>
        Task LogErrorAsync(
            string actionName,
            string errorMessage,
            string? module = null,
            string? description = null
        );

        // ═══════════════════════════════════════════════════
        // 📊 دوال الاستعلام
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// الحصول على السجلات مع الفلترة
        /// </summary>
        Task<List<ActivityLogViewModel>> GetLogsAsync(
            string? search = null,
            string? userId = null,
            string? actionType = null,
            string? module = null,
            string? severity = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int page = 1,
            int pageSize = 50
        );

        /// <summary>
        /// عدد السجلات الكلي مع الفلترة
        /// </summary>
        Task<int> GetLogsCountAsync(
            string? search = null,
            string? userId = null,
            string? actionType = null,
            string? module = null,
            string? severity = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null
        );

        /// <summary>
        /// الحصول على سجل واحد بالتفاصيل
        /// </summary>
        Task<ActivityLogViewModel?> GetLogByIdAsync(int id);

        /// <summary>
        /// الحصول على آخر N عملية لمستخدم معين
        /// </summary>
        Task<List<ActivityLogViewModel>> GetUserRecentActivityAsync(string userId, int count = 10);

        /// <summary>
        /// الحصول على آخر N عملية على سجل معين
        /// </summary>
        Task<List<ActivityLogViewModel>> GetEntityHistoryAsync(string entityName, int entityId, int count = 20);

        // ═══════════════════════════════════════════════════
        // 📈 إحصائيات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// عدد العمليات اليوم
        /// </summary>
        Task<int> GetTodayLogsCountAsync();

        /// <summary>
        /// عدد العمليات الحرجة
        /// </summary>
        Task<int> GetCriticalLogsCountAsync();

        /// <summary>
        /// عدد العمليات الفاشلة
        /// </summary>
        Task<int> GetFailedLogsCountAsync();

        /// <summary>
        /// الحصول على المستخدمين الذين لهم نشاطات (للفلتر)
        /// </summary>
        Task<List<UserSimpleVM>> GetActiveUsersAsync();

        /// <summary>
        /// الحصول على الأقسام المستخدمة (للفلتر)
        /// </summary>
        Task<List<string>> GetActiveModulesAsync();

        // ═══════════════════════════════════════════════════
        // 🗑️ صيانة
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// حذف سجل واحد
        /// </summary>
        Task<bool> DeleteLogAsync(int id);

        /// <summary>
        /// حذف السجلات الأقدم من تاريخ معين
        /// </summary>
        Task<int> DeleteOldLogsAsync(DateTime beforeDate);

        /// <summary>
        /// حذف كل السجلات
        /// </summary>
        Task<int> DeleteAllLogsAsync();
    }
}