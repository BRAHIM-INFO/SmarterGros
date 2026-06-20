using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmarterGros.Models
{
    /// <summary>
    /// 📦 بند فاتورة البيع (سطر واحد في الفاتورة)
    /// يمثل منتجاً مباعاً بكمية وسعر محددين
    /// </summary>
    public class SaleItem
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
        public int SaleId { get; set; }
        public Sale? Sale { get; set; }

        /// <summary>
        /// معرف المنتج
        /// </summary>
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        // ═══════════════════════════════════════════════════
        // 📊 الكميات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// الكمية المباعة
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// الكمية المرتجعة (للمرتجعات لاحقاً)
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
        /// سعر التكلفة (من المنتج وقت البيع)
        /// مهم لحساب الربح
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        /// <summary>
        /// نسبة الخصم على هذا البند (%)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal Discount { get; set; } = 0;

        /// <summary>
        /// نسبة الضريبة على هذا البند (%) - TVA
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal TaxRate { get; set; } = 0;

        /// <summary>
        /// المجموع الإجمالي للبند (بعد الخصم والضريبة)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// ربح هذا البند
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Profit { get; set; }

        // ═══════════════════════════════════════════════════
        // 📝 معلومات إضافية
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// ملاحظات على البند
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        // ═══════════════════════════════════════════════════
        // 🧮 Computed Properties
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
        /// المجموع بعد الخصم
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
        /// إجمالي التكلفة لهذا البند
        /// </summary>
        [NotMapped]
        public decimal TotalCost => Quantity * UnitCost;

        /// <summary>
        /// الكمية المتاحة للإرجاع
        /// </summary>
        [NotMapped]
        public int AvailableForReturn => Quantity - ReturnedQuantity;

        /// <summary>
        /// هل تم إرجاع كل الكمية؟
        /// </summary>
        [NotMapped]
        public bool IsFullyReturned => ReturnedQuantity >= Quantity;
    }
}