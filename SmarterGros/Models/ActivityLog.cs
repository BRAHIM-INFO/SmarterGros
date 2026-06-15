using System.ComponentModel.DataAnnotations;

namespace SmarterGros.Models
{
    /// <summary>
    /// سجل النشاطات - يخزن كل العمليات التي تحدث في النظام
    /// </summary>
    public class ActivityLog
    {
        public int Id { get; set; }

        // ═══════════════════════════════════════════════════
        // 👤 معلومات المستخدم
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// معرف المستخدم (من Identity)
        /// </summary>
        [MaxLength(450)]
        public string? UserId { get; set; }

        /// <summary>
        /// اسم المستخدم وقت العملية (للحفظ حتى لو حُذف المستخدم)
        /// </summary>
        [MaxLength(200)]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// الاسم الكامل للمستخدم
        /// </summary>
        [MaxLength(200)]
        public string? UserFullName { get; set; }

        /// <summary>
        /// دور المستخدم وقت العملية
        /// </summary>
        [MaxLength(100)]
        public string? UserRole { get; set; }

        // ═══════════════════════════════════════════════════
        // 🎯 معلومات العملية
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// نوع العملية (Create, Update, Delete, Login, Logout, View, Export, ...)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// اسم العملية بالعربية (مثل: "إضافة منتج", "حذف فاتورة")
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string ActionName { get; set; } = string.Empty;

        /// <summary>
        /// القسم المتأثر (Products, Sales, Purchases, Users, ...)
        /// </summary>
        [MaxLength(100)]
        public string? Module { get; set; }

        /// <summary>
        /// اسم الجدول/الـ Entity (مثل: Product, Sale, Category)
        /// </summary>
        [MaxLength(100)]
        public string? EntityName { get; set; }

        /// <summary>
        /// معرف السجل المتأثر (مثلاً: ID المنتج المُعدَّل)
        /// </summary>
        public int? EntityId { get; set; }

        /// <summary>
        /// وصف العملية بالعربية (مثل: "تم إضافة منتج جديد: حليب صافي")
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        // ═══════════════════════════════════════════════════
        // 📊 تفاصيل التغييرات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// القيم القديمة (JSON) - قبل التعديل
        /// </summary>
        public string? OldValues { get; set; }

        /// <summary>
        /// القيم الجديدة (JSON) - بعد التعديل
        /// </summary>
        public string? NewValues { get; set; }

        // ═══════════════════════════════════════════════════
        // 🌐 معلومات الاتصال
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// عنوان IP للمستخدم
        /// </summary>
        [MaxLength(50)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// معلومات المتصفح والجهاز
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// رابط الصفحة التي تمت منها العملية
        /// </summary>
        [MaxLength(500)]
        public string? RequestUrl { get; set; }

        /// <summary>
        /// نوع الطلب (GET, POST, PUT, DELETE)
        /// </summary>
        [MaxLength(10)]
        public string? RequestMethod { get; set; }

        // ═══════════════════════════════════════════════════
        // ⚠️ معلومات الحالة
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// هل العملية نجحت؟
        /// </summary>
        public bool IsSuccess { get; set; } = true;

        /// <summary>
        /// رسالة الخطأ (إن وُجد)
        /// </summary>
        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// مستوى الأهمية (Info, Warning, Critical)
        /// </summary>
        [MaxLength(20)]
        public string Severity { get; set; } = "Info";

        // ═══════════════════════════════════════════════════
        // 🕐 الوقت
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// وقت حدوث العملية
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// ثوابت أنواع العمليات
    /// </summary>
    public static class ActivityActionTypes
    {
        public const string Create = "Create";       // إضافة
        public const string Update = "Update";       // تعديل
        public const string Delete = "Delete";       // حذف
        public const string View = "View";           // عرض
        public const string Login = "Login";         // تسجيل دخول
        public const string Logout = "Logout";       // تسجيل خروج
        public const string Export = "Export";       // تصدير
        public const string Import = "Import";       // استيراد
        public const string Print = "Print";         // طباعة
        public const string Backup = "Backup";       // نسخ احتياطي
        public const string Restore = "Restore";     // استعادة
        public const string Failed = "Failed";       // فشل
    }

    /// <summary>
    /// ثوابت مستويات الأهمية
    /// </summary>
    public static class ActivitySeverity
    {
        public const string Info = "Info";           // معلومات عادية
        public const string Warning = "Warning";     // تحذير
        public const string Critical = "Critical";   // حرج (حذف، تغييرات مهمة)
    }
}