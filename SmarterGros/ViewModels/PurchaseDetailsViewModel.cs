using SmarterGros.Models;

namespace SmarterGros.ViewModels
{

    /// <summary>
    /// 👁️ ViewModel لعرض تفاصيل الفاتورة
    /// </summary>
    public class PurchaseDetailsViewModel
    {
        public Purchase Purchase { get; set; } = null!;
        public List<PurchaseReturn> Returns { get; set; } = new();
        public List<SupplierPayment> Payments { get; set; } = new();
        public List<ActivityLogViewModel> ActivityLogs { get; set; } = new();

        /// <summary>
        /// ✅ معلومات الشركة (للطباعة)
        /// </summary>
        public CompanySettings? CompanySettings { get; set; }

        // Computed Properties
        public decimal TotalPaid => Payments?.Sum(p => p.Amount) ?? 0;
        public decimal TotalReturned => Returns?.Where(r => !r.IsCancelled).Sum(r => r.TotalAmount) ?? 0;
        public decimal RemainingBalance => Purchase.GrandTotal - TotalPaid - TotalReturned;
        public int ReturnsCount => Returns?.Count(r => !r.IsCancelled) ?? 0;
        public int PaymentsCount => Payments?.Count ?? 0;
    }

    /// <summary>
    /// 👁️ ViewModel لعرض تفاصيل الفاتورة
    /// </summary>
    //public class PurchaseDetailsViewModel
    //{
    //    /// <summary>
    //    /// الفاتورة الكاملة
    //    /// </summary>
    //    public Purchase Purchase { get; set; } = null!;

    //    /// <summary>
    //    /// المرتجعات المرتبطة
    //    /// </summary>
    //    public List<PurchaseReturn> Returns { get; set; } = new();

    //    /// <summary>
    //    /// الدفعات المرتبطة
    //    /// </summary>
    //    public List<SupplierPayment> Payments { get; set; } = new();

    //    /// <summary>
    //    /// سجل الأنشطة على هذه الفاتورة
    //    /// </summary>
    //    public List<ActivityLogViewModel> ActivityLogs { get; set; } = new();

    //    // ═══════════════════════════════════════════════════
    //    // 🧮 Computed Properties
    //    // ═══════════════════════════════════════════════════

    //    /// <summary>
    //    /// إجمالي الدفعات
    //    /// </summary>
    //    public decimal TotalPaid => Payments?.Sum(p => p.Amount) ?? 0;

    //    /// <summary>
    //    /// إجمالي المرتجعات
    //    /// </summary>
    //    public decimal TotalReturned => Returns?.Sum(r => r.TotalAmount) ?? 0;

    //    /// <summary>
    //    /// الرصيد المتبقّي
    //    /// </summary>
    //    public decimal RemainingBalance
    //        => Purchase.GrandTotal - TotalPaid - TotalReturned;

    //    /// <summary>
    //    /// عدد المرتجعات
    //    /// </summary>
    //    public int ReturnsCount => Returns?.Count ?? 0;

    //    /// <summary>
    //    /// عدد الدفعات
    //    /// </summary>
    //    public int PaymentsCount => Payments?.Count ?? 0;
    //}
}