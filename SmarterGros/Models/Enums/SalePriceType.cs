namespace SmarterGros.Models.Enums
{
    /// <summary>
    /// 💰 نوع سعر البيع
    /// يحدد أي سعر سيُستخدم في الفاتورة (جملة/نصف جملة/تجزئة)
    /// </summary>
    public enum SalePriceType
    {
        /// <summary>
        /// 🟢 سعر التجزئة (للأفراد)
        /// الأعلى - للزبائن العاديين
        /// </summary>
        Retail = 1,

        /// <summary>
        /// 🟡 سعر نصف الجملة (B2B)
        /// المتوسط - للتجار الصغار
        /// </summary>
        SemiWholesale = 2,

        /// <summary>
        /// 🔵 سعر الجملة (Wholesale)
        /// الأقل - للتجار الكبار
        /// </summary>
        Wholesale = 3
    }

    /// <summary>
    /// 🛠️ Extension Methods لأنواع الأسعار
    /// </summary>
    public static class SalePriceTypeExtensions
    {
        public static string GetArabicName(this SalePriceType type)
        {
            return type switch
            {
                SalePriceType.Retail => "تجزئة",
                SalePriceType.SemiWholesale => "نصف جملة",
                SalePriceType.Wholesale => "جملة",
                _ => "غير معروف"
            };
        }

        public static string GetBadgeColor(this SalePriceType type)
        {
            return type switch
            {
                SalePriceType.Retail => "danger",       // أحمر
                SalePriceType.SemiWholesale => "warning", // أصفر
                SalePriceType.Wholesale => "info",        // أزرق
                _ => "secondary"
            };
        }

        public static string GetIcon(this SalePriceType type)
        {
            return type switch
            {
                SalePriceType.Retail => "fa-user",          // فرد
                SalePriceType.SemiWholesale => "fa-users",  // مجموعة
                SalePriceType.Wholesale => "fa-warehouse",  // مستودع
                _ => "fa-question"
            };
        }

        /// <summary>
        /// الحصول على السعر المناسب من المنتج
        /// </summary>
        public static decimal GetPriceFromProduct(this SalePriceType type, Product product)
        {
            return type switch
            {
                SalePriceType.Retail => product.RetailPriceTTC,
                SalePriceType.SemiWholesale => product.SemiWholesalePriceTTC,
                SalePriceType.Wholesale => product.WholesalePriceTTC,
                _ => product.RetailPriceTTC
            };
        }

        /// <summary>
        /// الحصول على السعر HT (بدون ضريبة)
        /// </summary>
        public static decimal GetPriceHTFromProduct(this SalePriceType type, Product product)
        {
            return type switch
            {
                SalePriceType.Retail => product.RetailPriceHT,
                SalePriceType.SemiWholesale => product.SemiWholesalePriceHT,
                SalePriceType.Wholesale => product.WholesalePriceHT,
                _ => product.RetailPriceHT
            };
        }
    }
}