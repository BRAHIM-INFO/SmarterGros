using SmarterGros.Models;

namespace SmarterGros.ViewModels
{
    /// <summary>
    /// 👁️ ViewModel لعرض تفاصيل الفاتورة
    /// </summary>
    public class SaleDetailsViewModel
    {
        /// <summary>
        /// الفاتورة الكاملة
        /// </summary>
        public Sale Sale { get; set; } = null!;

        /// <summary>
        /// الدفعات المرتبطة
        /// </summary>
        public List<CustomerPayment> Payments { get; set; } = new();

        /// <summary>
        /// سجل الأنشطة على هذه الفاتورة
        /// </summary>
        public List<ActivityLogViewModel> ActivityLogs { get; set; } = new();

        /// <summary>
        /// معلومات الشركة (للطباعة)
        /// </summary>
        public CompanySettings? CompanySettings { get; set; }

        // ═══════════════════════════════════════════════════
        // 🧮 Computed Properties
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// إجمالي الدفعات
        /// </summary>
        public decimal TotalPaid => Payments?.Sum(p => p.Amount) ?? 0;

        /// <summary>
        /// الرصيد المتبقّي
        /// </summary>
        public decimal RemainingBalance => Sale.TotalAmount - TotalPaid;

        /// <summary>
        /// عدد الدفعات
        /// </summary>
        public int PaymentsCount => Payments?.Count ?? 0;
    }
}