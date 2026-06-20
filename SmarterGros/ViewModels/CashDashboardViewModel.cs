using SmarterGros.Models;
using SmarterGros.Models.Enums;

namespace SmarterGros.ViewModels
{
    /// <summary>
    /// 📊 ViewModel للوحة الصندوق الرئيسية
    /// يحتوي على نظرة شاملة + آخر الحركات + إحصائيات
    /// </summary>
    public class CashDashboardViewModel
    {
        // ═══════════════════════════════════════════════════
        // 💰 الصندوق
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// الصندوق الافتراضي
        /// </summary>
        public CashRegister? CurrentRegister { get; set; }

        // ═══════════════════════════════════════════════════
        // 📊 إحصائيات اليوم
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// الرصيد الحالي
        /// </summary>
        public decimal CurrentBalance { get; set; }

        /// <summary>
        /// إجمالي واردات اليوم
        /// </summary>
        public decimal TodayIncome { get; set; }

        /// <summary>
        /// إجمالي صادرات اليوم
        /// </summary>
        public decimal TodayExpense { get; set; }

        /// <summary>
        /// عدد حركات اليوم
        /// </summary>
        public int TodayTransactionsCount { get; set; }

        // ═══════════════════════════════════════════════════
        // 📊 إحصائيات الأسبوع
        // ═══════════════════════════════════════════════════

        public decimal WeekIncome { get; set; }
        public decimal WeekExpense { get; set; }
        public int WeekTransactionsCount { get; set; }

        // ═══════════════════════════════════════════════════
        // 📊 إحصائيات الشهر
        // ═══════════════════════════════════════════════════

        public decimal MonthIncome { get; set; }
        public decimal MonthExpense { get; set; }
        public int MonthTransactionsCount { get; set; }

        // ═══════════════════════════════════════════════════
        // 📊 إحصائيات السنة
        // ═══════════════════════════════════════════════════

        public decimal YearIncome { get; set; }
        public decimal YearExpense { get; set; }
        public int YearTransactionsCount { get; set; }

        // ═══════════════════════════════════════════════════
        // 📜 آخر الحركات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// آخر 10 حركات
        /// </summary>
        public List<CashTransaction> RecentTransactions { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // 🔒 الجرد اليومي
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// هل اليوم مُغلق؟
        /// </summary>
        public bool IsTodayClosed { get; set; }

        /// <summary>
        /// جرد آخر يوم (إن وُجد)
        /// </summary>
        public DailyClosure? LastClosure { get; set; }

        // ═══════════════════════════════════════════════════
        // 📈 إحصائيات حسب الفئة
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// أعلى 5 فئات صرف هذا الشهر
        /// </summary>
        public List<CategoryStatsViewModel> TopExpenseCategories { get; set; } = new();

        /// <summary>
        /// أعلى 5 فئات دخل هذا الشهر
        /// </summary>
        public List<CategoryStatsViewModel> TopIncomeCategories { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // 🧮 Computed Properties
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// صافي اليوم
        /// </summary>
        public decimal TodayNet => TodayIncome - TodayExpense;

        /// <summary>
        /// صافي الأسبوع
        /// </summary>
        public decimal WeekNet => WeekIncome - WeekExpense;

        /// <summary>
        /// صافي الشهر
        /// </summary>
        public decimal MonthNet => MonthIncome - MonthExpense;

        /// <summary>
        /// صافي السنة
        /// </summary>
        public decimal YearNet => YearIncome - YearExpense;
    }

    /// <summary>
    /// 📊 إحصائيات حسب الفئة
    /// </summary>
    public class CategoryStatsViewModel
    {
        public TransactionCategory Category { get; set; }
        public string CategoryName => Category.GetArabicName();
        public string Icon => Category.GetIcon();
        public string Color => Category.GetColor();
        public decimal TotalAmount { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }
}