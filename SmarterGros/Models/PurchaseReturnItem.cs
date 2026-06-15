using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmarterGros.Models
{
    /// <summary>
    /// 📦 بند مرتجع شراء (سطر واحد في المرتجع)
    /// يمثل منتجاً مرتجعاً بكمية محددة
    /// </summary>
    public class PurchaseReturnItem
    {
        // ═══════════════════════════════════════════════════
        // 🔑 المفتاح الأساسي
        // ═══════════════════════════════════════════════════
        public int Id { get; set; }

        // ═══════════════════════════════════════════════════
        // 🔗 العلاقات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// معرف المرتجع الأم
        /// </summary>
        public int PurchaseReturnId { get; set; }
        public PurchaseReturn? PurchaseReturn { get; set; }

        /// <summary>
        /// معرف بند الفاتورة الأصلية (للربط والتتبع)
        /// </summary>
        public int PurchaseItemId { get; set; }
        public PurchaseItem? PurchaseItem { get; set; }

        /// <summary>
        /// معرف المنتج (لتسهيل الاستعلامات)
        /// </summary>
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        // ═══════════════════════════════════════════════════
        // 📊 الكميات والأسعار
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// الكمية المرتجعة
        /// </summary>
        public int ReturnedQuantity { get; set; }

        /// <summary>
        /// سعر الوحدة (نفس سعر الفاتورة الأصلية)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// نسبة الضريبة (TVA) - نفس البند الأصلي
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal TaxRate { get; set; } = 0;

        /// <summary>
        /// المجموع الإجمالي للبند (شامل الضريبة)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        // ═══════════════════════════════════════════════════
        // 📝 سبب الإرجاع
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// سبب إرجاع هذا البند تحديداً
        /// مثل: تالف / منتهي الصلاحية / خطأ في الطلب / غير مطابق للمواصفات
        /// </summary>
        [MaxLength(500)]
        public string? ReturnReason { get; set; }

        /// <summary>
        /// حالة المنتج المرتجع (سليم / تالف / منتهي الصلاحية)
        /// </summary>
        [MaxLength(100)]
        public string? ProductCondition { get; set; }

        /// <summary>
        /// ملاحظات على البند
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// رقم الدفعة (Batch Number) - من البند الأصلي
        /// </summary>
        [MaxLength(50)]
        public string? BatchNumber { get; set; }

        // ═══════════════════════════════════════════════════
        // 🧮 Computed Properties (محسوبة)
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// المجموع قبل الضريبة
        /// </summary>
        [NotMapped]
        public decimal SubTotal
            => ReturnedQuantity * UnitPrice;

        /// <summary>
        /// قيمة الضريبة
        /// </summary>
        [NotMapped]
        public decimal TaxAmount
            => SubTotal * (TaxRate / 100);
    }
}