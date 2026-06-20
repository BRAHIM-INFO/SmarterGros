namespace SmarterGros.ViewModels
{
    /// <summary>
    /// 📊 ViewModel لإحصائيات المبيعات
    /// </summary>
    public class SaleStatsViewModel
    {
        // ═══════════════════════════════════════════════════
        // 💰 الإحصائيات المالية
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// إجمالي المبيعات
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// إجمالي المبالغ المحصّلة
        /// </summary>
        public decimal TotalCollected { get; set; }

        /// <summary>
        /// إجمالي الديون على العملاء
        /// </summary>
        public decimal TotalDebt { get; set; }

        /// <summary>
        /// إجمالي قيمة المرتجعات
        /// </summary>
        public decimal TotalReturns { get; set; }

        /// <summary>
        /// إجمالي الأرباح (مهم!)
        /// </summary>
        public decimal TotalProfit { get; set; }

        /// <summary>
        /// إجمالي التكلفة
        /// </summary>
        public decimal TotalCost { get; set; }

        // ═══════════════════════════════════════════════════
        // 📊 إحصائيات العدد
        // ═══════════════════════════════════════════════════

        public int TotalCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        public int UnpaidCount { get; set; }
        public int ReturnsCount { get; set; }

        // ═══════════════════════════════════════════════════
        // 💳 إحصائيات نوع الدفع
        // ═══════════════════════════════════════════════════

        public int CashSalesCount { get; set; }
        public int CreditSalesCount { get; set; }
        public int PartialSalesCount { get; set; }

        public decimal CashSalesAmount { get; set; }
        public decimal CreditSalesAmount { get; set; }

        // ═══════════════════════════════════════════════════
        // 🧮 Computed Properties
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// نسبة التحصيل (%)
        /// </summary>
        public decimal CollectionPercentage
            => TotalAmount > 0 ? Math.Round((TotalCollected / TotalAmount) * 100, 2) : 0;

        /// <summary>
        /// نسبة الربح (%)
        /// </summary>
        public decimal ProfitPercentage
            => TotalCost > 0 ? Math.Round((TotalProfit / TotalCost) * 100, 2) : 0;

        /// <summary>
        /// الصافي بعد المرتجعات
        /// </summary>
        public decimal NetAmount => TotalAmount - TotalReturns;
    }
}