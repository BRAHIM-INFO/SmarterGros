namespace SmarterGros.Models.Enums
{
    /// <summary>
    /// 🚚 حالات الشحن (Transporteur)
    /// </summary>
    public enum ShippingStatus
    {
        /// <summary>
        /// ⏳ في انتظار الشحن
        /// </summary>
        Pending = 1,

        /// <summary>
        /// 🚚 في الطريق
        /// </summary>
        InTransit = 2,

        /// <summary>
        /// ✅ تم التسليم
        /// </summary>
        Delivered = 3,

        /// <summary>
        /// ⚠️ تأخر التسليم
        /// </summary>
        Delayed = 4,

        /// <summary>
        /// ❌ فشل التسليم
        /// </summary>
        Failed = 5
    }

    /// <summary>
    /// 🛠️ Extension Methods لحالات الشحن
    /// </summary>
    public static class ShippingStatusExtensions
    {
        public static string GetArabicName(this ShippingStatus status)
        {
            return status switch
            {
                ShippingStatus.Pending => "في الانتظار",
                ShippingStatus.InTransit => "في الطريق",
                ShippingStatus.Delivered => "تم التسليم",
                ShippingStatus.Delayed => "متأخر",
                ShippingStatus.Failed => "فشل التسليم",
                _ => "غير معروف"
            };
        }

        public static string GetBadgeColor(this ShippingStatus status)
        {
            return status switch
            {
                ShippingStatus.Pending => "secondary",
                ShippingStatus.InTransit => "info",
                ShippingStatus.Delivered => "success",
                ShippingStatus.Delayed => "warning",
                ShippingStatus.Failed => "danger",
                _ => "light"
            };
        }

        public static string GetIcon(this ShippingStatus status)
        {
            return status switch
            {
                ShippingStatus.Pending => "fa-clock",
                ShippingStatus.InTransit => "fa-truck-fast",
                ShippingStatus.Delivered => "fa-truck-ramp-box",
                ShippingStatus.Delayed => "fa-triangle-exclamation",
                ShippingStatus.Failed => "fa-truck-medical",
                _ => "fa-question"
            };
        }
    }
}