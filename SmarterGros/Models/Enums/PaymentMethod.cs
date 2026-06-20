namespace SmarterGros.Models.Enums
{
    /// <summary>
    /// 💳 طريقة الدفع
    /// تحدد كيف تمت الحركة المالية
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>
        /// 💵 نقدي - الأكثر شيوعاً
        /// </summary>
        Cash = 1,

        /// <summary>
        /// 📜 شيك بنكي
        /// </summary>
        Check = 2,

        /// <summary>
        /// 🏦 تحويل بنكي
        /// </summary>
        BankTransfer = 3,

        /// <summary>
        /// 💳 بطاقة بنكية
        /// </summary>
        Card = 4,

        /// <summary>
        /// 📱 دفع إلكتروني (موبايل)
        /// </summary>
        ElectronicPayment = 5
    }

    /// <summary>
    /// 🛠️ Extension Methods لطرق الدفع
    /// </summary>
    public static class PaymentMethodExtensions
    {
        public static string GetArabicName(this PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.Cash => "نقدي",
                PaymentMethod.Check => "شيك",
                PaymentMethod.BankTransfer => "تحويل بنكي",
                PaymentMethod.Card => "بطاقة بنكية",
                PaymentMethod.ElectronicPayment => "دفع إلكتروني",
                _ => "غير معروف"
            };
        }

        public static string GetIcon(this PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.Cash => "fa-money-bill-wave",
                PaymentMethod.Check => "fa-money-check",
                PaymentMethod.BankTransfer => "fa-building-columns",
                PaymentMethod.Card => "fa-credit-card",
                PaymentMethod.ElectronicPayment => "fa-mobile-screen",
                _ => "fa-question"
            };
        }

        public static string GetBadgeColor(this PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.Cash => "success",
                PaymentMethod.Check => "warning",
                PaymentMethod.BankTransfer => "info",
                PaymentMethod.Card => "primary",
                PaymentMethod.ElectronicPayment => "purple",
                _ => "secondary"
            };
        }
    }
}