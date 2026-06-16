using System.ComponentModel.DataAnnotations;
using SmarterGros.Models.Enums;

namespace SmarterGros.ViewModels
{
    /// <summary>
    /// 📦 ViewModel لاستلام فاتورة شراء
    /// </summary>
    public class PurchaseReceiveViewModel
    {
        [Required]
        public int PurchaseId { get; set; }

        /// <summary>
        /// رقم الفاتورة (للعرض)
        /// </summary>
        public string? InvoiceNumber { get; set; }

        /// <summary>
        /// تاريخ الاستلام الفعلي
        /// </summary>
        [Required(ErrorMessage = "تاريخ الاستلام مطلوب")]
        [Display(Name = "تاريخ الاستلام")]
        [DataType(DataType.Date)]
        public DateTime ReceivedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// حالة الشحن النهائية
        /// </summary>
        [Display(Name = "حالة الشحن")]
        public ShippingStatus ShippingStatus { get; set; } = ShippingStatus.Delivered;

        /// <summary>
        /// البنود المستلمة (الكميات الفعلية)
        /// </summary>
        public List<PurchaseReceiveItemViewModel> Items { get; set; } = new();

        /// <summary>
        /// ملاحظات الاستلام
        /// </summary>
        [Display(Name = "ملاحظات الاستلام")]
        [MaxLength(1000)]
        public string? ReceivingNotes { get; set; }

        /// <summary>
        /// تحديث سعر التكلفة في المنتج تلقائياً؟
        /// </summary>
        [Display(Name = "تحديث أسعار المنتجات")]
        public bool UpdateProductPrices { get; set; } = false;
    }

    /// <summary>
    /// 📦 بند الاستلام (الكمية الفعلية)
    /// </summary>
    public class PurchaseReceiveItemViewModel
    {
        public int PurchaseItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// الكمية المطلوبة في الفاتورة
        /// </summary>
        public int OrderedQuantity { get; set; }

        /// <summary>
        /// الكمية المستلمة فعلياً
        /// </summary>
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون موجبة")]
        public int ReceivedQuantity { get; set; }

        /// <summary>
        /// ملاحظات على البند
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}