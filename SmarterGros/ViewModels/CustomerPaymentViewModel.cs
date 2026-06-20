using System.ComponentModel.DataAnnotations;

namespace SmarterGros.ViewModels
{
    /// <summary>
    /// 💳 ViewModel لتسجيل دفعة من عميل
    /// </summary>
    public class CustomerPaymentViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "العميل مطلوب")]
        [Display(Name = "العميل")]
        public int CustomerId { get; set; }

        /// <summary>
        /// اسم العميل (للعرض)
        /// </summary>
        public string? CustomerName { get; set; }

        /// <summary>
        /// الفاتورة المرتبطة (اختياري)
        /// </summary>
        [Display(Name = "الفاتورة")]
        public int? SaleId { get; set; }

        /// <summary>
        /// رقم الفاتورة (للعرض)
        /// </summary>
        public string? InvoiceNumber { get; set; }

        /// <summary>
        /// المبلغ المستحق على الفاتورة
        /// </summary>
        public decimal? AmountDue { get; set; }

        /// <summary>
        /// إجمالي دين العميل الحالي
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