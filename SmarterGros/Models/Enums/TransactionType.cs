namespace SmarterGros.Models.Enums
{
    /// <summary>
    /// 💰 نوع الحركة المالية
    /// تحدد ما إذا كانت الحركة دخل (وارد) أو خرج (صادر)
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// 🟢 وارد - دخل للصندوق
        /// مثل: مبيعات، تحصيل، إيرادات
        /// </summary>
        Income = 1,

        /// <summary>
        /// 🔴 صادر - خرج من الصندوق
        /// مثل: مشتريات، مصاريف، رواتب
        /// </summary>
        Expense = 2
    }

    /// <summary>
    /// 🛠️ Extension Methods لنوع الحركة
    /// </summary>
    public static class TransactionTypeExtensions
    {
        public static string GetArabicName(this TransactionType type)
        {
            return type switch
            {
                TransactionType.Income => "وارد",
                TransactionType.Expense => "صادر",
                _ => "غير معروف"
            };
        }

        public static string GetBadgeColor(this TransactionType type)
        {
            return type switch
            {
                TransactionType.Income => "success",
                TransactionType.Expense => "danger",
                _ => "secondary"
            };
        }

        public static string GetIcon(this TransactionType type)
        {
            return type switch
            {
                TransactionType.Income => "fa-arrow-down",
                TransactionType.Expense => "fa-arrow-up",
                _ => "fa-question"
            };
        }

        public static string GetSign(this TransactionType type)
        {
            return type switch
            {
                TransactionType.Income => "+",
                TransactionType.Expense => "-",
                _ => ""
            };
        }

        public static string GetColor(this TransactionType type)
        {
            return type switch
            {
                TransactionType.Income => "#28a745",  // أخضر
                TransactionType.Expense => "#dc3545", // أحمر
                _ => "#6c757d"
            };
        }
    }
}