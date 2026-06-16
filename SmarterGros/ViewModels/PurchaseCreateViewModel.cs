using System.ComponentModel.DataAnnotations;
using SmarterGros.Models.Enums;

namespace SmarterGros.ViewModels
{
    /// <summary>
    /// 🆕 ViewModel لإنشاء/تعديل فاتورة شراء
    /// </summary>
    public class PurchaseCreateViewModel
    {
        // ═══════════════════════════════════════════════════
        // 🔑 المعرّف (للتعديل فقط)
        // ═══════════════════════════════════════════════════
        public int? Id { get; set; }

        // ═══════════════════════════════════════════════════
        // 📋 معلومات الفاتورة
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// رقم الفاتورة (يُولَّد تلقائياً)
        /// </summary>
        public string? InvoiceNumber { get; set; }

        /// <summary>
        /// رقم فاتورة المورد (اختياري)
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "رقم فاتورة المورد")]
        public string? SupplierInvoiceNumber { get; set; }

        /// <summary>
        /// تاريخ الفاتورة
        /// </summary>
        [Required(ErrorMessage = "تاريخ الفاتورة مطلوب")]
        [Display(Name = "تاريخ الفاتورة")]
        [DataType(DataType.Date)]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        // ═══════════════════════════════════════════════════
        // 🚚 المورد
        // ═══════════════════════════════════════════════════

        [Required(ErrorMessage = "اختيار المورد مطلوب")]
        [Display(Name = "المورد")]
        public int SupplierId { get; set; }

        // ═══════════════════════════════════════════════════
        // 📦 البنود
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// قائمة المنتجات في الفاتورة
        /// </summary>
        [Required(ErrorMessage = "يجب إضافة منتج واحد على الأقل")]
        public List<PurchaseItemCreateViewModel> Items { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // 💰 الخصم والضريبة
        // ═══════════════════════════════════════════════════

        [Display(Name = "نسبة الخصم (%)")]
        [Range(0, 100, ErrorMessage = "نسبة الخصم بين 0 و 100")]
        public decimal DiscountPercentage { get; set; } = 0;

        [Display(Name = "قيمة الخصم")]
        [Range(0, double.MaxValue, ErrorMessage = "قيمة الخصم يجب أن تكون موجبة")]
        public decimal Discount { get; set; } = 0;

        // ═══════════════════════════════════════════════════
        // 🚚 معلومات الشحن
        // ═══════════════════════════════════════════════════

        [Display(Name = "اسم الناقل")]
        [MaxLength(200)]
        public string? TransporterName { get; set; }

        [Display(Name = "هاتف الناقل")]
        [MaxLength(20)]
        public string? TransporterPhone { get; set; }

        [Display(Name = "رقم وصل الشحن (BL)")]
        [MaxLength(50)]
        public string? DeliveryNoteNumber { get; set; }

        [Display(Name = "تكلفة الشحن")]
        [Range(0, double.MaxValue)]
        public decimal ShippingCost { get; set; } = 0;

        [Display(Name = "تاريخ الشحن")]
        [DataType(DataType.Date)]
        public DateTime? ShippingDate { get; set; }

        [Display(Name = "حالة الشحن")]
        public ShippingStatus? ShippingStatus { get; set; }

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

        // ═══════════════════════════════════════════════════
        // 📊 حالة الفاتورة (Draft / Sent)
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// حفظ كمسودة (لا تأثير على المخزون)
        /// </summary>
        public bool SaveAsDraft { get; set; } = true;
    }

    /// <summary>
    /// 📦 ViewModel لبند في الفاتورة عند الإنشاء
    /// </summary>
    public class PurchaseItemCreateViewModel
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
        [Display(Name = "سعر الوحدة")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "نسبة الخصم (%)")]
        [Range(0, 100)]
        public decimal Discount { get; set; } = 0;

        [Display(Name = "نسبة الضريبة (%)")]
        [Range(0, 100)]
        public decimal TaxRate { get; set; } = 0;

        [Display(Name = "ملاحظات")]
        [MaxLength(500)]
        public string? Notes { get; set; }

        [Display(Name = "رقم الدفعة")]
        [MaxLength(50)]
        public string? BatchNumber { get; set; }

        [Display(Name = "تاريخ انتهاء الدفعة")]
        [DataType(DataType.Date)]
        public DateTime? BatchExpiryDate { get; set; }
    }
}