namespace SmarterGros.Models.Enums
{
    /// <summary>
    /// 📂 فئة الحركة المالية
    /// تصنف الحركات حسب نوعها (مبيعات، مشتريات، مصاريف...)
    /// </summary>
    public enum TransactionCategory
    {
        // ═══════════════════════════════════════════════════
        // 🟢 الواردات (Income Categories)
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// 💰 مبيعات نقدية
        /// </summary>
        Sale = 1,

        /// <summary>
        /// 💵 تحصيل من عميل (سداد دين)
        /// </summary>
        CustomerPayment = 2,

        /// <summary>
        /// 🔄 استرداد من مورد (مرتجع نقدي)
        /// </summary>
        SupplierRefund = 3,

        /// <summary>
        /// 🏦 إيداع بنكي (دخول كاش من البنك)
        /// </summary>
        BankDeposit = 4,

        /// <summary>
        /// 💎 رأس مال
        /// </summary>
        Capital = 5,

        /// <summary>
        /// 📈 إيراد آخر
        /// </summary>
        OtherIncome = 6,

        // ═══════════════════════════════════════════════════
        // 🔴 الصادرات (Expense Categories)
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// 🛒 شراء نقدي
        /// </summary>
        Purchase = 10,

        /// <summary>
        /// 💸 دفع لمورد (سداد دين)
        /// </summary>
        SupplierPayment = 11,

        /// <summary>
        /// 🔄 استرداد لعميل (مرتجع نقدي)
        /// </summary>
        CustomerRefund = 12,

        /// <summary>
        /// ⚡ كهرباء
        /// </summary>
        Electricity = 13,

        /// <summary>
        /// 💧 ماء
        /// </summary>
        Water = 14,

        /// <summary>
        /// 📞 اتصالات / إنترنت
        /// </summary>
        Communications = 15,

        /// <summary>
        /// 🏠 إيجار
        /// </summary>
        Rent = 16,

        /// <summary>
        /// 👨‍💼 رواتب
        /// </summary>
        Salary = 17,

        /// <summary>
        /// 🚚 نقل / مواصلات
        /// </summary>
        Transportation = 18,

        /// <summary>
        /// 📦 صيانة
        /// </summary>
        Maintenance = 19,

        /// <summary>
        /// 🏦 سحب نقدي (إيداع في البنك)
        /// </summary>
        BankWithdrawal = 20,

        /// <summary>
        /// 💳 سداد قرض
        /// </summary>
        LoanPayment = 21,

        /// <summary>
        /// 📊 ضرائب
        /// </summary>
        Tax = 22,

        /// <summary>
        /// 🎁 مصروف آخر
        /// </summary>
        OtherExpense = 23
    }

    /// <summary>
    /// 🛠️ Extension Methods لفئات الحركات
    /// </summary>
    public static class TransactionCategoryExtensions
    {
        public static string GetArabicName(this TransactionCategory category)
        {
            return category switch
            {
                // الواردات
                TransactionCategory.Sale => "مبيعات نقدية",
                TransactionCategory.CustomerPayment => "تحصيل من عميل",
                TransactionCategory.SupplierRefund => "استرداد من مورد",
                TransactionCategory.BankDeposit => "إيداع بنكي",
                TransactionCategory.Capital => "رأس مال",
                TransactionCategory.OtherIncome => "إيراد آخر",

                // الصادرات
                TransactionCategory.Purchase => "شراء نقدي",
                TransactionCategory.SupplierPayment => "دفع لمورد",
                TransactionCategory.CustomerRefund => "استرداد لعميل",
                TransactionCategory.Electricity => "كهرباء",
                TransactionCategory.Water => "ماء",
                TransactionCategory.Communications => "اتصالات",
                TransactionCategory.Rent => "إيجار",
                TransactionCategory.Salary => "رواتب",
                TransactionCategory.Transportation => "نقل",
                TransactionCategory.Maintenance => "صيانة",
                TransactionCategory.BankWithdrawal => "سحب للبنك",
                TransactionCategory.LoanPayment => "سداد قرض",
                TransactionCategory.Tax => "ضرائب",
                TransactionCategory.OtherExpense => "مصروف آخر",

                _ => "غير معروف"
            };
        }

        public static string GetIcon(this TransactionCategory category)
        {
            return category switch
            {
                // الواردات
                TransactionCategory.Sale => "fa-cash-register",
                TransactionCategory.CustomerPayment => "fa-hand-holding-usd",
                TransactionCategory.SupplierRefund => "fa-rotate-left",
                TransactionCategory.BankDeposit => "fa-building-columns",
                TransactionCategory.Capital => "fa-coins",
                TransactionCategory.OtherIncome => "fa-plus-circle",

                // الصادرات
                TransactionCategory.Purchase => "fa-shopping-cart",
                TransactionCategory.SupplierPayment => "fa-money-bill-wave",
                TransactionCategory.CustomerRefund => "fa-rotate-left",
                TransactionCategory.Electricity => "fa-bolt",
                TransactionCategory.Water => "fa-droplet",
                TransactionCategory.Communications => "fa-phone",
                TransactionCategory.Rent => "fa-house",
                TransactionCategory.Salary => "fa-user-tie",
                TransactionCategory.Transportation => "fa-truck",
                TransactionCategory.Maintenance => "fa-wrench",
                TransactionCategory.BankWithdrawal => "fa-piggy-bank",
                TransactionCategory.LoanPayment => "fa-credit-card",
                TransactionCategory.Tax => "fa-file-invoice-dollar",
                TransactionCategory.OtherExpense => "fa-minus-circle",

                _ => "fa-question"
            };
        }

        public static string GetColor(this TransactionCategory category)
        {
            // الواردات (أخضر)
            if ((int)category < 10)
                return "#28a745";

            // الصادرات (ألوان متعددة حسب النوع)
            return category switch
            {
                TransactionCategory.Purchase => "#dc3545",
                TransactionCategory.SupplierPayment => "#dc3545",
                TransactionCategory.Electricity => "#ffc107",
                TransactionCategory.Water => "#17a2b8",
                TransactionCategory.Communications => "#6610f2",
                TransactionCategory.Rent => "#fd7e14",
                TransactionCategory.Salary => "#e83e8c",
                TransactionCategory.Transportation => "#20c997",
                _ => "#6c757d"
            };
        }

        /// <summary>
        /// تحديد نوع الحركة بناءً على الفئة
        /// </summary>
        public static TransactionType GetTransactionType(this TransactionCategory category)
        {
            // الفئات 1-9 = واردات
            // الفئات 10+ = صادرات
            return (int)category < 10 ? TransactionType.Income : TransactionType.Expense;
        }

        /// <summary>
        /// قائمة فئات الواردات فقط
        /// </summary>
        public static List<TransactionCategory> GetIncomeCategories()
        {
            return new List<TransactionCategory>
            {
                TransactionCategory.Sale,
                TransactionCategory.CustomerPayment,
                TransactionCategory.SupplierRefund,
                TransactionCategory.BankDeposit,
                TransactionCategory.Capital,
                TransactionCategory.OtherIncome
            };
        }

        /// <summary>
        /// قائمة فئات الصادرات فقط
        /// </summary>
        public static List<TransactionCategory> GetExpenseCategories()
        {
            return new List<TransactionCategory>
            {
                TransactionCategory.Purchase,
                TransactionCategory.SupplierPayment,
                TransactionCategory.CustomerRefund,
                TransactionCategory.Electricity,
                TransactionCategory.Water,
                TransactionCategory.Communications,
                TransactionCategory.Rent,
                TransactionCategory.Salary,
                TransactionCategory.Transportation,
                TransactionCategory.Maintenance,
                TransactionCategory.BankWithdrawal,
                TransactionCategory.LoanPayment,
                TransactionCategory.Tax,
                TransactionCategory.OtherExpense
            };
        }
    }
}