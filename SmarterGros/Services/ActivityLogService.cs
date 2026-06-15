using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmarterGros.Data;
using SmarterGros.Models;
using SmarterGros.ViewModels;
using System.Text.Json;

namespace SmarterGros.Services
{
    /// <summary>
    /// خدمة تسجيل النشاطات - التنفيذ الفعلي
    /// </summary>
    public class ActivityLogService : IActivityLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public ActivityLogService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        // ═══════════════════════════════════════════════════
        // 📝 دوال التسجيل
        // ═══════════════════════════════════════════════════

        public async Task LogAsync(
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
            string? errorMessage = null)
        {
            try
            {
                var log = new ActivityLog
                {
                    ActionType = actionType,
                    ActionName = actionName,
                    Module = module,
                    EntityName = entityName,
                    EntityId = entityId,
                    Description = description,
                    OldValues = SerializeObject(oldValues),
                    NewValues = SerializeObject(newValues),
                    Severity = severity,
                    IsSuccess = isSuccess,
                    ErrorMessage = errorMessage,
                    CreatedAt = DateTime.Now
                };

                // إضافة معلومات المستخدم
                await AddUserInfoAsync(log);

                // إضافة معلومات الاتصال
                AddRequestInfo(log);

                _context.ActivityLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // لا نريد أن يفشل البرنامج بسبب فشل التسجيل
                // فقط نطبع الخطأ في Console
                Console.WriteLine($"❌ فشل تسجيل النشاط: {ex.Message}");
            }
        }

        public async Task LogCreateAsync(
            string module,
            string entityName,
            int entityId,
            string description,
            object? newValues = null)
        {
            await LogAsync(
                actionType: ActivityActionTypes.Create,
                actionName: $"إضافة {GetEntityArabicName(entityName)}",
                module: module,
                entityName: entityName,
                entityId: entityId,
                description: description,
                newValues: newValues,
                severity: ActivitySeverity.Info
            );
        }

        public async Task LogUpdateAsync(
            string module,
            string entityName,
            int entityId,
            string description,
            object? oldValues = null,
            object? newValues = null)
        {
            await LogAsync(
                actionType: ActivityActionTypes.Update,
                actionName: $"تعديل {GetEntityArabicName(entityName)}",
                module: module,
                entityName: entityName,
                entityId: entityId,
                description: description,
                oldValues: oldValues,
                newValues: newValues,
                severity: ActivitySeverity.Info
            );
        }

        public async Task LogDeleteAsync(
            string module,
            string entityName,
            int entityId,
            string description,
            object? deletedData = null)
        {
            await LogAsync(
                actionType: ActivityActionTypes.Delete,
                actionName: $"حذف {GetEntityArabicName(entityName)}",
                module: module,
                entityName: entityName,
                entityId: entityId,
                description: description,
                oldValues: deletedData,
                severity: ActivitySeverity.Critical  // الحذف دائماً Critical
            );
        }

        public async Task LogViewAsync(string module, string? description = null)
        {
            await LogAsync(
                actionType: ActivityActionTypes.View,
                actionName: "عرض",
                module: module,
                description: description,
                severity: ActivitySeverity.Info
            );
        }

        public async Task LogLoginAsync(string userName, bool isSuccess, string? errorMessage = null)
        {
            var log = new ActivityLog
            {
                UserName = userName,
                ActionType = isSuccess ? ActivityActionTypes.Login : ActivityActionTypes.Failed,
                ActionName = isSuccess ? "تسجيل دخول ناجح" : "محاولة دخول فاشلة",
                Module = "Account",
                Description = isSuccess
                    ? $"تسجيل دخول للمستخدم: {userName}"
                    : $"فشل تسجيل الدخول للمستخدم: {userName} - {errorMessage}",
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage,
                Severity = isSuccess ? ActivitySeverity.Info : ActivitySeverity.Warning,
                CreatedAt = DateTime.Now
            };

            // إذا نجح الدخول، نحصل على معلومات المستخدم
            if (isSuccess)
            {
                var user = await _userManager.FindByNameAsync(userName);
                if (user != null)
                {
                    log.UserId = user.Id;
                    log.UserFullName = user.FullName;
                    log.UserRole = user.Role;
                }
            }

            AddRequestInfo(log);

            try
            {
                _context.ActivityLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ فشل تسجيل الدخول: {ex.Message}");
            }
        }

        public async Task LogLogoutAsync()
        {
            await LogAsync(
                actionType: ActivityActionTypes.Logout,
                actionName: "تسجيل خروج",
                module: "Account",
                description: "تسجيل خروج من النظام",
                severity: ActivitySeverity.Info
            );
        }

        public async Task LogErrorAsync(
            string actionName,
            string errorMessage,
            string? module = null,
            string? description = null)
        {
            await LogAsync(
                actionType: ActivityActionTypes.Failed,
                actionName: actionName,
                module: module,
                description: description,
                isSuccess: false,
                errorMessage: errorMessage,
                severity: ActivitySeverity.Warning
            );
        }

        // ═══════════════════════════════════════════════════
        // 📊 دوال الاستعلام
        // ═══════════════════════════════════════════════════

        public async Task<List<ActivityLogViewModel>> GetLogsAsync(
            string? search = null,
            string? userId = null,
            string? actionType = null,
            string? module = null,
            string? severity = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int page = 1,
            int pageSize = 50)
        {
            var query = _context.ActivityLogs.AsQueryable();

            // تطبيق الفلاتر
            query = ApplyFilters(query, search, userId, actionType, module, severity, dateFrom, dateTo);

            // الترتيب من الأحدث للأقدم
            query = query.OrderByDescending(l => l.CreatedAt);

            // Pagination
            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            var logs = await query.ToListAsync();

            return logs.Select(MapToViewModel).ToList();
        }

        public async Task<int> GetLogsCountAsync(
            string? search = null,
            string? userId = null,
            string? actionType = null,
            string? module = null,
            string? severity = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            var query = _context.ActivityLogs.AsQueryable();
            query = ApplyFilters(query, search, userId, actionType, module, severity, dateFrom, dateTo);
            return await query.CountAsync();
        }

        public async Task<ActivityLogViewModel?> GetLogByIdAsync(int id)
        {
            var log = await _context.ActivityLogs.FindAsync(id);
            return log == null ? null : MapToViewModel(log);
        }

        public async Task<List<ActivityLogViewModel>> GetUserRecentActivityAsync(string userId, int count = 10)
        {
            var logs = await _context.ActivityLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .Take(count)
                .ToListAsync();

            return logs.Select(MapToViewModel).ToList();
        }

        public async Task<List<ActivityLogViewModel>> GetEntityHistoryAsync(string entityName, int entityId, int count = 20)
        {
            var logs = await _context.ActivityLogs
                .Where(l => l.EntityName == entityName && l.EntityId == entityId)
                .OrderByDescending(l => l.CreatedAt)
                .Take(count)
                .ToListAsync();

            return logs.Select(MapToViewModel).ToList();
        }

        // ═══════════════════════════════════════════════════
        // 📈 إحصائيات
        // ═══════════════════════════════════════════════════

        public async Task<int> GetTodayLogsCountAsync()
        {
            var today = DateTime.Today;
            return await _context.ActivityLogs
                .Where(l => l.CreatedAt >= today)
                .CountAsync();
        }

        public async Task<int> GetCriticalLogsCountAsync()
        {
            return await _context.ActivityLogs
                .Where(l => l.Severity == ActivitySeverity.Critical)
                .CountAsync();
        }

        public async Task<int> GetFailedLogsCountAsync()
        {
            return await _context.ActivityLogs
                .Where(l => !l.IsSuccess)
                .CountAsync();
        }

        public async Task<List<UserSimpleVM>> GetActiveUsersAsync()
        {
            // الحصول على IDs المستخدمين النشطين من السجلات
            var userIds = await _context.ActivityLogs
                .Where(l => l.UserId != null)
                .Select(l => l.UserId!)
                .Distinct()
                .ToListAsync();

            if (!userIds.Any())
                return new List<UserSimpleVM>();

            // جلب المستخدمين بطريقة بسيطة
            var allUsers = await _userManager.Users.ToListAsync();

            var users = allUsers
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new UserSimpleVM
                {
                    Id = u.Id,
                    FullName = u.FullName ?? u.UserName ?? ""
                })
                .OrderBy(u => u.FullName)
                .ToList();

            return users;
        }

        public async Task<List<string>> GetActiveModulesAsync()
        {
            return await _context.ActivityLogs
                .Where(l => l.Module != null)
                .Select(l => l.Module!)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();
        }

        // ═══════════════════════════════════════════════════
        // 🗑️ صيانة
        // ═══════════════════════════════════════════════════

        public async Task<bool> DeleteLogAsync(int id)
        {
            var log = await _context.ActivityLogs.FindAsync(id);
            if (log == null) return false;

            _context.ActivityLogs.Remove(log);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> DeleteOldLogsAsync(DateTime beforeDate)
        {
            var oldLogs = await _context.ActivityLogs
                .Where(l => l.CreatedAt < beforeDate)
                .ToListAsync();

            int count = oldLogs.Count;

            if (count > 0)
            {
                _context.ActivityLogs.RemoveRange(oldLogs);
                await _context.SaveChangesAsync();
            }

            return count;
        }

        public async Task<int> DeleteAllLogsAsync()
        {
            int count = await _context.ActivityLogs.CountAsync();
            _context.ActivityLogs.RemoveRange(_context.ActivityLogs);
            await _context.SaveChangesAsync();
            return count;
        }

        // ═══════════════════════════════════════════════════
        // 🛠️ دوال مساعدة (Private)
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// إضافة معلومات المستخدم الحالي
        /// </summary>
        private async Task AddUserInfoAsync(ActivityLog log)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var appUser = await _userManager.GetUserAsync(user);
                if (appUser != null)
                {
                    log.UserId = appUser.Id;
                    log.UserName = appUser.UserName ?? "";
                    log.UserFullName = appUser.FullName;
                    log.UserRole = appUser.Role;
                }
            }
            else
            {
                log.UserName = "نظام";
            }
        }

        /// <summary>
        /// إضافة معلومات الطلب (IP, User Agent, URL)
        /// </summary>
        private void AddRequestInfo(ActivityLog log)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            // IP Address
            log.IpAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            // إذا كان خلف Proxy
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                log.IpAddress = forwardedFor.Split(',')[0].Trim();
            }

            // User Agent
            log.UserAgent = httpContext.Request.Headers["User-Agent"].ToString();
            if (log.UserAgent?.Length > 500)
                log.UserAgent = log.UserAgent.Substring(0, 500);

            // Request URL
            log.RequestUrl = $"{httpContext.Request.Path}{httpContext.Request.QueryString}";
            if (log.RequestUrl?.Length > 500)
                log.RequestUrl = log.RequestUrl.Substring(0, 500);

            // Request Method
            log.RequestMethod = httpContext.Request.Method;
        }

        /// <summary>
        /// تحويل كائن إلى JSON
        /// </summary>
        private string? SerializeObject(object? obj)
        {
            if (obj == null) return null;

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                return JsonSerializer.Serialize(obj, options);
            }
            catch
            {
                return obj.ToString();
            }
        }

        /// <summary>
        /// تحويل اسم الـ Entity للعربية
        /// </summary>
        private string GetEntityArabicName(string entityName)
        {
            return entityName switch
            {
                "Product" => "منتج",
                "Category" => "فئة",
                "Supplier" => "مورد",
                "Customer" => "عميل",
                "Purchase" => "فاتورة شراء",
                "Sale" => "فاتورة بيع",
                "StockMovement" => "حركة مخزون",
                "ApplicationUser" => "مستخدم",
                "CompanySettings" => "إعدادات المؤسسة",
                "SupplierPayment" => "دفعة مورد",
                "CustomerPayment" => "دفعة عميل",
                _ => entityName
            };
        }

        /// <summary>
        /// تحويل ActivityLog إلى ViewModel
        /// </summary>
        private ActivityLogViewModel MapToViewModel(ActivityLog log)
        {
            return new ActivityLogViewModel
            {
                Id = log.Id,
                UserId = log.UserId,
                UserName = log.UserName,
                UserFullName = log.UserFullName,
                UserRole = log.UserRole,
                ActionType = log.ActionType,
                ActionName = log.ActionName,
                Module = log.Module,
                EntityName = log.EntityName,
                EntityId = log.EntityId,
                Description = log.Description,
                OldValues = log.OldValues,
                NewValues = log.NewValues,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                RequestUrl = log.RequestUrl,
                RequestMethod = log.RequestMethod,
                IsSuccess = log.IsSuccess,
                ErrorMessage = log.ErrorMessage,
                Severity = log.Severity,
                CreatedAt = log.CreatedAt
            };
        }

        /// <summary>
        /// تطبيق الفلاتر على الـ Query
        /// </summary>
        private IQueryable<ActivityLog> ApplyFilters(
            IQueryable<ActivityLog> query,
            string? search,
            string? userId,
            string? actionType,
            string? module,
            string? severity,
            DateTime? dateFrom,
            DateTime? dateTo)
        {
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(l =>
                    l.UserName.Contains(search) ||
                    (l.UserFullName != null && l.UserFullName.Contains(search)) ||
                    l.ActionName.Contains(search) ||
                    (l.Description != null && l.Description.Contains(search)));
            }

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(l => l.UserId == userId);

            if (!string.IsNullOrEmpty(actionType))
                query = query.Where(l => l.ActionType == actionType);

            if (!string.IsNullOrEmpty(module))
                query = query.Where(l => l.Module == module);

            if (!string.IsNullOrEmpty(severity))
                query = query.Where(l => l.Severity == severity);

            if (dateFrom.HasValue)
                query = query.Where(l => l.CreatedAt >= dateFrom.Value);

            if (dateTo.HasValue)
            {
                var endOfDay = dateTo.Value.Date.AddDays(1);
                query = query.Where(l => l.CreatedAt < endOfDay);
            }

            return query;
        }
    }
}