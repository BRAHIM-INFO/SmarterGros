namespace SmarterGros.ViewModels
{
    /// <summary>
    /// ViewModel لعرض سجل النشاطات في القائمة
    /// </summary>
    public class ActivityLogViewModel
    {
        public int Id { get; set; }

        // معلومات المستخدم
        public string? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserFullName { get; set; }
        public string? UserRole { get; set; }

        // معلومات العملية
        public string ActionType { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public string? Module { get; set; }
        public string? EntityName { get; set; }
        public int? EntityId { get; set; }
        public string? Description { get; set; }

        // التغييرات
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }

        // معلومات الاتصال
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? RequestUrl { get; set; }
        public string? RequestMethod { get; set; }

        // الحالة
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string Severity { get; set; } = "Info";

        // الوقت
        public DateTime CreatedAt { get; set; }

        // ═══════════════════════════════════════════════════
        // 🎨 خصائص العرض المساعدة
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// الأيقونة حسب نوع العملية
        /// </summary>
        public string GetActionIcon()
        {
            return ActionType switch
            {
                "Create" => "fa-plus-circle",
                "Update" => "fa-edit",
                "Delete" => "fa-trash",
                "View" => "fa-eye",
                "Login" => "fa-sign-in-alt",
                "Logout" => "fa-sign-out-alt",
                "Export" => "fa-file-export",
                "Import" => "fa-file-import",
                "Print" => "fa-print",
                "Backup" => "fa-download",
                "Restore" => "fa-upload",
                "Failed" => "fa-exclamation-triangle",
                _ => "fa-circle"
            };
        }

        /// <summary>
        /// اللون حسب نوع العملية
        /// </summary>
        public string GetActionColor()
        {
            return ActionType switch
            {
                "Create" => "#4caf50",       // أخضر
                "Update" => "#2196f3",       // أزرق
                "Delete" => "#f44336",       // أحمر
                "View" => "#9e9e9e",         // رمادي
                "Login" => "#00bcd4",        // سماوي
                "Logout" => "#607d8b",       // رمادي مزرق
                "Export" => "#ff9800",       // برتقالي
                "Import" => "#673ab7",       // بنفسجي
                "Print" => "#795548",        // بني
                "Backup" => "#3f51b5",       // نيلي
                "Restore" => "#009688",      // تركواز
                "Failed" => "#f44336",       // أحمر
                _ => "#757575"
            };
        }

        /// <summary>
        /// اسم العملية بالعربية
        /// </summary>
        public string GetActionTypeArabic()
        {
            return ActionType switch
            {
                "Create" => "إضافة",
                "Update" => "تعديل",
                "Delete" => "حذف",
                "View" => "عرض",
                "Login" => "دخول",
                "Logout" => "خروج",
                "Export" => "تصدير",
                "Import" => "استيراد",
                "Print" => "طباعة",
                "Backup" => "نسخ احتياطي",
                "Restore" => "استعادة",
                "Failed" => "فشل",
                _ => ActionType
            };
        }

        /// <summary>
        /// لون مستوى الأهمية
        /// </summary>
        public string GetSeverityColor()
        {
            return Severity switch
            {
                "Info" => "#2196f3",
                "Warning" => "#ff9800",
                "Critical" => "#f44336",
                _ => "#757575"
            };
        }

        /// <summary>
        /// اسم مستوى الأهمية بالعربية
        /// </summary>
        public string GetSeverityArabic()
        {
            return Severity switch
            {
                "Info" => "معلومات",
                "Warning" => "تحذير",
                "Critical" => "حرج",
                _ => Severity
            };
        }

        /// <summary>
        /// الوقت بصيغة "منذ ..."
        /// </summary>
        public string GetTimeAgo()
        {
            var span = DateTime.Now - CreatedAt;

            if (span.TotalSeconds < 60)
                return "الآن";
            if (span.TotalMinutes < 60)
                return $"منذ {(int)span.TotalMinutes} دقيقة";
            if (span.TotalHours < 24)
                return $"منذ {(int)span.TotalHours} ساعة";
            if (span.TotalDays < 7)
                return $"منذ {(int)span.TotalDays} يوم";
            if (span.TotalDays < 30)
                return $"منذ {(int)(span.TotalDays / 7)} أسبوع";
            if (span.TotalDays < 365)
                return $"منذ {(int)(span.TotalDays / 30)} شهر";

            return $"منذ {(int)(span.TotalDays / 365)} سنة";
        }

        /// <summary>
        /// أول حرف من اسم المستخدم (للـ Avatar)
        /// </summary>
        public string GetUserInitial()
        {
            if (!string.IsNullOrEmpty(UserFullName))
                return UserFullName.Substring(0, 1).ToUpper();
            if (!string.IsNullOrEmpty(UserName))
                return UserName.Substring(0, 1).ToUpper();
            return "?";
        }
    }

    /// <summary>
    /// ViewModel للفلاتر والإحصائيات في صفحة Index
    /// </summary>
    public class ActivityLogIndexViewModel
    {
        public List<ActivityLogViewModel> Logs { get; set; } = new();

        // فلاتر
        public string? Search { get; set; }
        public string? UserId { get; set; }
        public string? ActionType { get; set; }
        public string? Module { get; set; }
        public string? Severity { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        // قوائم منسدلة
        public List<UserSimpleVM> Users { get; set; } = new();
        public List<string> ActionTypes { get; set; } = new();
        public List<string> Modules { get; set; } = new();

        // إحصائيات
        public int TotalLogs { get; set; }
        public int TodayLogs { get; set; }
        public int CriticalLogs { get; set; }
        public int FailedLogs { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// نموذج بسيط لعرض المستخدم في الفلتر
    /// </summary>
    public class UserSimpleVM
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}