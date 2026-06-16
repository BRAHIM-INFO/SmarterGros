using System.ComponentModel.DataAnnotations;
using SmarterGros.Models.Enums;

namespace SmarterGros.ViewModels
{
    /// <summary>
    /// 🆕 ViewModel لإنشاء مرتجع شراء
    /// </summary>
    public class PurchaseReturnCreateViewModel
    {
        public int? Id { get; set; }

        // ═══════════════════════════════════════════════════
        // 🔗 الفاتورة الأصلية
        // ═══════════════════════════════════════════════════

        [Required(ErrorMessage = "الفاتورة مطلوبة")]
        [Display(Name = "الفاتورة الأصلية")]
        public int PurchaseId { get; set; }

        /// <summary>
        /// رقم الفاتورة (للعرض)
        /// </summary>
        public string? InvoiceNumber { get; set; }

        /// <summary>
        /// معرف المورد (يُملأ تلقائياً من الفاتورة)
        /// </summary>
        public int SupplierId { get; set; }

        /// <summary>
        /// اسم المورد (للعرض)
        /// </summary>
        public string? SupplierName { get; set; }

        // ═══════════════════════════════════════════════════
        // 📋 معلومات المرتجع
        // ═══════════════════════════════════════════════════

        [Required(ErrorMessage = "تاريخ المرتجع مطلوب")]
        [Display(Name = "تاريخ المرتجع")]
        [DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "سبب الإرجاع مطلوب")]
        [Display(Name = "سبب الإرجاع")]
        [MaxLength(500)]
        public string ReturnReason { get; set; } = string.Empty;

        // ═══════════════════════════════════════════════════
        // 📦 البنود المرتجعة
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// قائمة البنود المرتجعة
        /// </summary>
        public List<PurchaseReturnItemCreateViewModel> Items { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // 💳 طريقة الاسترداد
        // ═══════════════════════════════════════════════════

        [Required]
        [Display(Name = "طريقة الاسترداد")]
        public ReturnRefundMethod RefundMethod { get; set; }
            = ReturnRefundMethod.DeductFromDebt;

        [Display(Name = "المبلغ المخصوم من الدين")]
        [Range(0, double.MaxValue)]
        public decimal DeductedFromDebt { get; set; } = 0;

        [Display(Name = "المبلغ المسترد نقداً")]
        [Range(0, double.MaxValue)]
        public decimal CashRefunded { get; set; } = 0;

        // ═══════════════════════════════════════════════════
        // 📝 ملاحظات
        // ═══════════════════════════════════════════════════

        [Display(Name = "ملاحظات")]
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// 📦 بند في المرتجع
    /// </summary>
    public class PurchaseReturnItemCreateViewModel
    {
        /// <summary>
        /// معرف بند الفاتورة الأصلية
        /// </summary>
        [Required]
        public int PurchaseItemId { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// الكمية المستلمة (للعرض)
        /// </summary>
        public int ReceivedQuantity { get; set; }

        /// <summary>
        /// الكمية المرتجعة سابقاً
        /// </summary>
        public int PreviouslyReturned { get; set; }

        /// <summary>
        /// الكمية المتاحة للإرجاع
        /// </summary>
        public int AvailableForReturn => ReceivedQuantity - PreviouslyReturned;

        /// <summary>
        /// الكمية المراد إرجاعها الآن
        /// </summary>
        [Required]
        [Range(0, int.MaxValue)]
        [Display(Name = "الكمية المرتجعة")]
        public int ReturnedQuantity { get; set; }

        /// <summary>
        /// سعر الوحدة (من الفاتورة الأصلية)
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// نسبة الضريبة
        /// </summary>
        public decimal TaxRate { get; set; } = 0;

        /// <summary>
        /// سبب إرجاع هذا البند
        /// </summary>
        [Display(Name = "سبب الإرجاع")]
        [MaxLength(500)]
        public string? ReturnReason { get; set; }

        /// <summary>
        /// حالة المنتج
        /// </summary>
        [Display(Name = "حالة المنتج")]
        [MaxLength(100)]
        public string? ProductCondition { get; set; }

        /// <summary>
        /// رقم الدفعة (من البند الأصلي)
        /// </summary>
        public string? BatchNumber { get; set; }
    }
}