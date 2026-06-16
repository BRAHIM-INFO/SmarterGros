using System.ComponentModel.DataAnnotations;

namespace SmarterGros.ViewModels
{
    /// <summary>
    /// 💳 ViewModel لتسجيل دفعة لمورد
    /// </summary>
    public class SupplierPaymentViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "المورد مطلوب")]
        [Display(Name = "المورد")]
        public int SupplierId { get; set; }

        /// <summary>
        /// اسم المورد (للعرض)
        /// </summary>
        public string? SupplierName { get; set; }

        /// <summary>
        /// الفاتورة المرتبطة (اختياري - للدفعات على فاتورة محددة)
        /// </summary>
        [Display(Name = "الفاتورة")]
        public int? PurchaseId { get; set; }

        /// <summary>
        /// رقم الفاتورة (للعرض)
        /// </summary>
        public string? InvoiceNumber { get; set; }

        /// <summary>
        /// المبلغ المستحق على الفاتورة
        /// </summary>
        public decimal? AmountDue { get; set; }

        /// <summary>
        /// إجمالي دين المورد الحالي
        /// </summary>
        public decimal CurrentDebt { get; set; }

        // ═══════════════════════════════════════════════════
        // 💰 بيانات الدفعة
        // ═══════════════════════════════════════════════════

        [Required(ErrorMessage = "المبلغ مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من 0")]
        [Display(Name = "المبلغ")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "تاريخ الدفعة مطلوب")]
        [Display(Name = "تاريخ الدفعة")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Display(Name = "ملاحظات")]
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}