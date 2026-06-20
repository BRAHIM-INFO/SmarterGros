using System.ComponentModel.DataAnnotations;
using SmarterGros.Models.Enums;

namespace SmarterGros.ViewModels
{
    /// <summary>
    /// 🆕 ViewModel لإنشاء/تعديل فاتورة بيع
    /// </summary>
    public class SaleCreateViewModel
    {
        public int? Id { get; set; }

        // ═══════════════════════════════════════════════════
        // 📋 معلومات الفاتورة
        // ═══════════════════════════════════════════════════

        public string? InvoiceNumber { get; set; }

        [Required(ErrorMessage = "تاريخ البيع مطلوب")]
        [Display(Name = "تاريخ البيع")]
        [DataType(DataType.Date)]
        public DateTime SaleDate { get; set; } = DateTime.Now;

        // ═══════════════════════════════════════════════════
        // 👥 العميل
        // ═══════════════════════════════════════════════════

        [Display(Name = "العميل")]
        public int? CustomerId { get; set; }

        /// <summary>
        /// اسم العميل (للبيع النقدي بدون تسجيل)
        /// </summary>
        [Display(Name = "اسم العميل (إذا غير مسجل)")]
        [MaxLength(200)]
        public string? CustomerName { get; set; }

        // ═══════════════════════════════════════════════════
        // 💰 نوع السعر
        // ═══════════════════════════════════════════════════

        [Required(ErrorMessage = "نوع السعر مطلوب")]
        [Display(Name = "نوع السعر")]
        public SalePriceType PriceType { get; set; } = SalePriceType.Retail;

        // ═══════════════════════════════════════════════════
        // 📦 البنود
        // ═══════════════════════════════════════════════════

        [Required(ErrorMessage = "يجب إضافة منتج واحد على الأقل")]
        public List<SaleItemCreateViewModel> Items { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // 💰 الخصم
        // ═══════════════════════════════════════════════════

        [Display(Name = "نسبة الخصم (%)")]
        [Range(0, 100)]
        public decimal DiscountPercentage { get; set; } = 0;

        [Display(Name = "قيمة الخصم")]
        [Range(0, double.MaxValue)]
        public decimal Discount { get; set; } = 0;

        // ═══════════════════════════════════════════════════
        // 💳 الدفع
        // ═══════════════════════════════════════════════════

        [Required(ErrorMessage = "نوع الدفع مطلوب")]
        [Display(Name = "نوع الدفع")]
        public PaymentType PaymentType { get; set; } = PaymentType.Cash;

        [Display(Name = "المبلغ المدفوع")]
        [Range(0, double.MaxValue)]
        public decimal PaidAmount { get; set; } = 0;

        // ═══════════════════════════════════════════════════
        // 📝 ملاحظات
        // ═══════════════════════════════════════════════════

        [Display(Name = "ملاحظات")]
        [MaxLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// حفظ كمسودة (لا تأثير على المخزون/الصندوق)
        /// </summary>
        public bool SaveAsDraft { get; set; } = false;
    }

    /// <summary>
    /// 📦 ViewModel لبند في الفاتورة عند الإنشاء
    /// </summary>
    public class SaleItemCreateViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "المنتج مطلوب")]
        [Display(Name = "المنتج")]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من 0")]
        [Display(Name = "الكمية")]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "السعر يجب أن يكون أكبر من 0")]
        [Display(Name = "سعر البيع")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// سعر التكلفة (يُملأ تلقائياً من المنتج)
        /// </summary>
        public decimal UnitCost { get; set; }

        [Display(Name = "نسبة الخصم (%)")]
        [Range(0, 100)]
        public decimal Discount { get; set; } = 0;

        [Display(Name = "نسبة الضريبة (%)")]
        [Range(0, 100)]
        public decimal TaxRate { get; set; } = 0;

        [Display(Name = "ملاحظات")]
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}