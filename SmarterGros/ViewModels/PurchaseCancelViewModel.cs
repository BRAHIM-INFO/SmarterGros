using System.ComponentModel.DataAnnotations;

namespace SmarterGros.ViewModels
{
    /// <summary>
    /// ❌ ViewModel لإلغاء فاتورة شراء
    /// </summary>
    public class PurchaseCancelViewModel
    {
        [Required]
        public int PurchaseId { get; set; }

        /// <summary>
        /// رقم الفاتورة (للعرض)
        /// </summary>
        public string? InvoiceNumber { get; set; }

        /// <summary>
        /// سبب الإلغاء
        /// </summary>
        [Required(ErrorMessage = "سبب الإلغاء مطلوب")]
        [Display(Name = "سبب الإلغاء")]
        [MaxLength(500)]
        public string CancellationReason { get; set; } = string.Empty;

        /// <summary>
        /// تأكيد عكس التأثيرات (المخزون + الدين)
        /// </summary>
        [Required]
        [Display(Name = "أؤكد عكس التأثيرات على المخزون والدين")]
        public bool ConfirmReversal { get; set; }
    }
}