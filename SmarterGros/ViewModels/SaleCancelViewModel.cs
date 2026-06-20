using System.ComponentModel.DataAnnotations;

namespace SmarterGros.ViewModels
{
    /// <summary>
    /// ❌ ViewModel لإلغاء فاتورة بيع
    /// </summary>
    public class SaleCancelViewModel
    {
        [Required]
        public int SaleId { get; set; }

        public string? InvoiceNumber { get; set; }

        [Required(ErrorMessage = "سبب الإلغاء مطلوب")]
        [Display(Name = "سبب الإلغاء")]
        [MaxLength(500)]
        public string CancellationReason { get; set; } = string.Empty;

        /// <summary>
        /// تأكيد عكس التأثيرات
        /// </summary>
        [Required]
        [Display(Name = "أؤكد عكس التأثيرات على المخزون والصندوق")]
        public bool ConfirmReversal { get; set; }
    }
}