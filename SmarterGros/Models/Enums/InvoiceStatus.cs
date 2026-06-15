namespace SmarterGros.Models.Enums
{
    /// <summary>
    /// 📋 حالات فاتورة الشراء
    /// تحدد المرحلة التي وصلت إليها الفاتورة في دورة حياتها
    /// </summary>
    public enum InvoiceStatus
    {
        /// <summary>
        /// 📝 مسودة - الفاتورة قيد الإعداد
        /// قابلة للتعديل والحذف - لا تأثير على المخزون
        /// </summary>
        Draft = 1,

        /// <summary>
        /// 📤 مرسلة - تم تأكيدها وفي انتظار الاستلام
        /// لا تأثير على المخزون بعد
        /// </summary>
        Sent = 2,

        /// <summary>
        /// ✅ مستلمة - تم استلام البضاعة فعلياً
        /// تأثير كامل: زيادة المخزون + تحديث رصيد المورد
        /// </summary>
        Received = 3,

        /// <summary>
        /// ❌ ملغاة - تم إلغاء الفاتورة
        /// عكس كل التأثيرات
        /// </summary>
        Cancelled = 4
    }

    /// <summary>
    /// 🛠️ Extension Methods للحصول على معلومات العرض
    /// تسهّل علينا الاستخدام في الـ Views
    /// </summary>
    public static class InvoiceStatusExtensions
    {
        /// <summary>
        /// الاسم العربي للحالة
        /// </summary>
        public static string GetArabicName(this InvoiceStatus status)
        {
            return status switch
            {
                InvoiceStatus.Draft => "مسودة",
                InvoiceStatus.Sent => "مرسلة",
                InvoiceStatus.Received => "مستلمة",
                InvoiceStatus.Cancelled => "ملغاة",
                _ => "غير معروف"
            };
        }

        /// <summary>
        /// لون البادج (Bootstrap)
        /// </summary>
        public static string GetBadgeColor(this InvoiceStatus status)
        {
            return status switch
            {
                InvoiceStatus.Draft => "secondary",
                InvoiceStatus.Sent => "warning",
                InvoiceStatus.Received => "success",
                InvoiceStatus.Cancelled => "danger",
                _ => "light"
            };
        }

        /// <summary>
        /// أيقونة Font Awesome
        /// </summary>
        public static string GetIcon(this InvoiceStatus status)
        {
            return status switch
            {
                InvoiceStatus.Draft => "fa-pen-to-square",
                InvoiceStatus.Sent => "fa-paper-plane",
                InvoiceStatus.Received => "fa-circle-check",
                InvoiceStatus.Cancelled => "fa-circle-xmark",
                _ => "fa-question"
            };
        }

        /// <summary>
        /// هل يمكن تعديل الفاتورة؟
        /// </summary>
        public static bool CanEdit(this InvoiceStatus status)
        {
            return status == InvoiceStatus.Draft;
        }

        /// <summary>
        /// هل يمكن حذف الفاتورة؟
        /// </summary>
        public static bool CanDelete(this InvoiceStatus status)
        {
            return status == InvoiceStatus.Draft;
        }

        /// <summary>
        /// هل يمكن استلام الفاتورة؟
        /// </summary>
        public static bool CanReceive(this InvoiceStatus status)
        {
            return status == InvoiceStatus.Draft || status == InvoiceStatus.Sent;
        }

        /// <summary>
        /// هل يمكن إلغاء الفاتورة؟
        /// </summary>
        public static bool CanCancel(this InvoiceStatus status)
        {
            return status != InvoiceStatus.Cancelled;
        }

        /// <summary>
        /// هل يمكن إنشاء مرتجع لهذه الفاتورة؟
        /// </summary>
        public static bool CanReturn(this InvoiceStatus status)
        {
            return status == InvoiceStatus.Received;
        }
    }
}