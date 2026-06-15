namespace SmarterGros.Models.Enums
{
    /// <summary>
    /// 💰 أنواع الدفع للفاتورة
    /// </summary>
    public enum PaymentType
    {
        /// <summary>
        /// 💵 نقداً - دفع كامل المبلغ فوراً
        /// يُسجَّل في SupplierPayments كدفعة كاملة
        /// </summary>
        Cash = 1,

        /// <summary>
        /// 📋 كريدي - كل المبلغ دين على الشركة
        /// لا تسجيل في SupplierPayments
        /// </summary>
        Credit = 2,

        /// <summary>
        /// 🔀 جزئي - جزء نقدي + جزء كريدي
        /// يُسجَّل المبلغ المدفوع فقط في SupplierPayments
        /// </summary>
        Partial = 3
    }

    /// <summary>
    /// 🛠️ Extension Methods لأنواع الدفع
    /// </summary>
    public static class PaymentTypeExtensions
    {
        public static string GetArabicName(this PaymentType type)
        {
            return type switch
            {
                PaymentType.Cash => "نقداً",
                PaymentType.Credit => "كريدي (آجل)",
                PaymentType.Partial => "دفع جزئي",
                _ => "غير معروف"
            };
        }

        public static string GetBadgeColor(this PaymentType type)
        {
            return type switch
            {
                PaymentType.Cash => "success",
                PaymentType.Credit => "danger",
                PaymentType.Partial => "warning",
                _ => "secondary"
            };
        }

        public static string GetIcon(this PaymentType type)
        {
            return type switch
            {
                PaymentType.Cash => "fa-money-bill-wave",
                PaymentType.Credit => "fa-credit-card",
                PaymentType.Partial => "fa-coins",
                _ => "fa-question"
            };
        }

        /// <summary>
        /// هل يتطلب تسجيل دفعة في SupplierPayments؟
        /// </summary>
        public static bool RequiresPaymentRecord(this PaymentType type)
        {
            return type == PaymentType.Cash || type == PaymentType.Partial;
        }
    }
}