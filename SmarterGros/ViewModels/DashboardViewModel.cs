using SmarterGros.Models;

namespace SmarterGros.ViewModels
{
    /// <summary>
    /// 📊 ViewModel للوحة التحكم الرئيسية
    /// شامل لكل المعلومات والإحصائيات
    /// </summary>
    public class DashboardViewModel
    {
        // ═══════════════════════════════════════════════════
        // 👤 معلومات المستخدم
        // ═══════════════════════════════════════════════════

        public string UserFullName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public DateTime CurrentDateTime { get; set; } = DateTime.Now;

        // ═══════════════════════════════════════════════════
        // 💰 الأداء المالي اليوم
        // ═══════════════════════════════════════════════════

        public FinancialPerformance TodayPerformance { get; set; } = new();
        public FinancialPerformance YesterdayPerformance { get; set; } = new();
        public FinancialPerformance MonthPerformance { get; set; } = new();
        public FinancialPerformance YearPerformance { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // 📊 KPIs الأساسية
        // ═══════════════════════════════════════════════════

        public KpiCard ProductsCount { get; set; } = new();
        public KpiCard CustomersCount { get; set; } = new();
        public KpiCard SuppliersCount { get; set; } = new();
        public KpiCard CashBalance { get; set; } = new();
        public KpiCard CustomersDebt { get; set; } = new();
        public KpiCard SuppliersDebt { get; set; } = new();
        public KpiCard LowStockCount { get; set; } = new();
        public KpiCard ExpiringProductsCount { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // 📈 الرسوم البيانية
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// بيانات الرسم البياني للمبيعات/المشتريات (آخر 7 أيام)
        /// </summary>
        public List<DailyTransactionData> Last7DaysData { get; set; } = new();

        /// <summary>
        /// بيانات الرسم البياني للأرباح (آخر 30 يوم)
        /// </summary>
        public List<DailyProfitData> Last30DaysProfit { get; set; } = new();

        /// <summary>
        /// توزيع المبيعات حسب الفئة (هذا الشهر)
        /// </summary>
        public List<CategoryDistribution> SalesByCategory { get; set; } = new();

        /// <summary>
        /// توزيع الصندوق (واردات vs صادرات)
        /// </summary>
        public List<CashFlowData> CashFlowMonth { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // 🏆 أعلى وأهم
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// أعلى 5 منتجات مبيعاً (هذا الشهر)
        /// </summary>
        public List<TopProduct> TopSellingProducts { get; set; } = new();

        /// <summary>
        /// أعلى 5 عملاء (هذا الشهر)
        /// </summary>
        public List<TopCustomer> TopCustomers { get; set; } = new();

        /// <summary>
        /// أعلى 5 موردين
        /// </summary>
        public List<TopSupplier> TopSuppliers { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // 📋 آخر العمليات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// آخر 5 فواتير بيع
        /// </summary>
        public List<RecentInvoice> RecentSales { get; set; } = new();

        /// <summary>
        /// آخر 5 فواتير شراء
        /// </summary>
        public List<RecentInvoice> RecentPurchases { get; set; } = new();

        /// <summary>
        /// آخر 5 حركات صندوق
        /// </summary>
        public List<RecentCashTransaction> RecentCashTransactions { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // ⚠️ التنبيهات
        // ═══════════════════════════════════════════════════

        public List<DashboardAlert> Alerts { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // 🧮 Computed Properties
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// تحية ديناميكية حسب الوقت
        /// </summary>
        public string Greeting
        {
            get
            {
                var hour = CurrentDateTime.Hour;
                if (hour >= 5 && hour < 12) return "🌅 صباح الخير";
                if (hour >= 12 && hour < 17) return "☀️ ظهر سعيد";
                if (hour >= 17 && hour < 21) return "🌆 مساء النور";
                return "🌙 ليلة سعيدة";
            }
        }

        /// <summary>
        /// نسبة التغير في المبيعات (اليوم vs الأمس)
        /// </summary>
        public decimal SalesChangePercentage
        {
            get
            {
                if (YesterdayPerformance.SalesAmount == 0) return 0;
                return Math.Round(
                    ((TodayPerformance.SalesAmount - YesterdayPerformance.SalesAmount) / YesterdayPerformance.SalesAmount) * 100,
                    2);
            }
        }

        /// <summary>
        /// هل تجاوزنا أداء الأمس؟
        /// </summary>
        public bool IsTodayBetterThanYesterday
            => TodayPerformance.NetProfit > YesterdayPerformance.NetProfit;
    }
}