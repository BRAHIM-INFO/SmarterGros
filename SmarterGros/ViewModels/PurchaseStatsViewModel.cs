namespace SmarterGros.ViewModels
{
    /// <summary>
    /// 📊 ViewModel لإحصائيات المشتريات
    /// يستخدم في البطاقات الملونة أعلى الصفحة
    /// </summary>
    public class PurchaseStatsViewModel
    {
        // ═══════════════════════════════════════════════════
        // 💰 الإحصائيات المالية
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// إجمالي قيمة المشتريات
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// إجمالي المبالغ المدفوعة
        /// </summary>
        public decimal TotalPaid { get; set; }

        /// <summary>
        /// إجمالي الديون المستحقة
        /// </summary>
        public decimal TotalDebt { get; set; }

        /// <summary>
        /// إجمالي قيمة المرتجعات
        /// </summary>
        public decimal TotalReturns { get; set; }

        /// <summary>
        /// إجمالي تكاليف الشحن
        /// </summary>
        public decimal TotalShipping { get; set; }

        // ═══════════════════════════════════════════════════
        // 📊 إحصائيات العدد
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// إجمالي عدد الفواتير
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// عدد الفواتير - مسودة
        /// </summary>
        public int DraftCount { get; set; }

        /// <summary>
        /// عدد الفواتير - مرسلة
        /// </summary>
        public int SentCount { get; set; }

        /// <summary>
        /// عدد الفواتير - مستلمة
        /// </summary>
        public int ReceivedCount { get; set; }

        /// <summary>
        /// عدد الفواتير - ملغاة
        /// </summary>
        public int CancelledCount { get; set; }

        /// <summary>
        /// عدد الفواتير غير المسددة
        /// </summary>
        public int UnpaidCount { get; set; }

        /// <summary>
        /// عدد المرتجعات
        /// </summary>
        public int ReturnsCount { get; set; }

        // ═══════════════════════════════════════════════════
        // 🧮 Computed Properties
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// نسبة الدفع (من 0 إلى 100)
        /// </summary>
        public decimal PaymentPercentage
            => TotalAmount > 0 ? Math.Round((TotalPaid / TotalAmount) * 100, 2) : 0;

        /// <summary>
        /// الصافي بعد المرتجعات
        /// </summary>
        public decimal NetAmount => TotalAmount - TotalReturns;
    }
}