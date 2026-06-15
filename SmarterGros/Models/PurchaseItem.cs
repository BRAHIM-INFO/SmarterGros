using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmarterGros.Models
{
    /// <summary>
    /// 📦 بند فاتورة الشراء (سطر واحد في الفاتورة)
    /// يمثل منتجاً واحداً بكمية وسعر محددين
    /// </summary>
    public class PurchaseItem
    {
        // ═══════════════════════════════════════════════════
        // 🔑 المفتاح الأساسي
        // ═══════════════════════════════════════════════════
        public int Id { get; set; }

        // ═══════════════════════════════════════════════════
        // 🔗 العلاقات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// معرف الفاتورة الأم
        /// </summary>
        public int PurchaseId { get; set; }
        public Purchase? Purchase { get; set; }

        /// <summary>
        /// معرف المنتج
        /// </summary>
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        // ═══════════════════════════════════════════════════
        // 📊 الكميات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// الكمية المطلوبة (في الفاتورة الأصلية)
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// الكمية المستلمة فعلياً
        /// قد تختلف عن Quantity إذا كان الاستلام جزئياً
        /// </summary>
        public int ReceivedQuantity { get; set; } = 0;

        /// <summary>
        /// الكمية المرتجعة (للمرتجعات)
        /// </summary>
        public int ReturnedQuantity { get; set; } = 0;

        // ═══════════════════════════════════════════════════
        // 💰 الأسعار
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// سعر الوحدة (بدون ضريبة - HT)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// نسبة الخصم على هذا البند (%)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal Discount { get; set; } = 0;

        /// <summary>
        /// نسبة الضريبة على هذا البند (%) - مثل: 19% TVA
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal TaxRate { get; set; } = 0;

        /// <summary>
        /// المجموع الإجمالي للبند (بعد الخصم والضريبة)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        // ═══════════════════════════════════════════════════
        // 📝 معلومات إضافية
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// ملاحظات على هذا البند (مثل: تالف، منتهي الصلاحية...)
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// تاريخ انتهاء الصلاحية للدفعة (إن وجد)
        /// مهم للمواد الغذائية والأدوية
        /// </summary>
        public DateTime? BatchExpiryDate { get; set; }

        /// <summary>
        /// رقم الدفعة (Batch Number) - للتتبع
        /// </summary>
        [MaxLength(50)]
        public string? BatchNumber { get; set; }

        // ═══════════════════════════════════════════════════
        // 🧮 Computed Properties (محسوبة - غير محفوظة في DB)
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// المجموع قبل الخصم والضريبة
        /// </summary>
        [NotMapped]
        public decimal SubTotal => Quantity * UnitPrice;

        /// <summary>
        /// قيمة الخصم بالدج
        /// </summary>
        [NotMapped]
        public decimal DiscountAmount
            => SubTotal * (Discount / 100);

        /// <summary>
        /// المجموع بعد الخصم (قبل الضريبة)
        /// </summary>
        [NotMapped]
        public decimal SubTotalAfterDiscount
            => SubTotal - DiscountAmount;

        /// <summary>
        /// قيمة الضريبة بالدج
        /// </summary>
        [NotMapped]
        public decimal TaxAmount
            => SubTotalAfterDiscount * (TaxRate / 100);

        /// <summary>
        /// الكمية المتبقية (غير مستلمة)
        /// </summary>
        [NotMapped]
        public int RemainingQuantity
            => Quantity - ReceivedQuantity;

        /// <summary>
        /// الكمية المتاحة للإرجاع (مستلمة - مرتجعة)
        /// </summary>
        [NotMapped]
        public int AvailableForReturn
            => ReceivedQuantity - ReturnedQuantity;

        /// <summary>
        /// هل البند مستلم بالكامل؟
        /// </summary>
        [NotMapped]
        public bool IsFullyReceived
            => ReceivedQuantity >= Quantity;

        /// <summary>
        /// هل تم إرجاع كل الكمية المستلمة؟
        /// </summary>
        [NotMapped]
        public bool IsFullyReturned
            => ReturnedQuantity >= ReceivedQuantity && ReceivedQuantity > 0;
    }
}