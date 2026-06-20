using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmarterGros.Models.Enums;

namespace SmarterGros.Models
{
    /// <summary>
    /// 💰 فاتورة البيع (Facture de Vente)
    /// تمثل عملية بيع كاملة لعميل
    /// </summary>
    public class Sale
    {
        // ═══════════════════════════════════════════════════
        // 🔑 المفتاح الأساسي
        // ═══════════════════════════════════════════════════
        public int Id { get; set; }

        // ═══════════════════════════════════════════════════
        // 📋 معلومات الفاتورة الأساسية
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// رقم الفاتورة (مثال: SAL-2026-00001)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        /// <summary>
        /// تاريخ البيع
        /// </summary>
        public DateTime SaleDate { get; set; } = DateTime.Now;

        // ═══════════════════════════════════════════════════
        // 👥 العميل
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// العميل (اختياري - للبيع النقدي السريع)
        /// </summary>
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        /// <summary>
        /// اسم العميل (إذا كان "عميل نقدي" بدون تسجيل)
        /// </summary>
        [MaxLength(200)]
        public string? CustomerName { get; set; }

        // ═══════════════════════════════════════════════════
        // 💰 نوع السعر
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// نوع السعر المُستخدم (جملة/نصف جملة/تجزئة)
        /// </summary>
        public SalePriceType PriceType { get; set; } = SalePriceType.Retail;

        // ═══════════════════════════════════════════════════
        // 💵 المبالغ المالية
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// المجموع قبل الضريبة والخصم
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        /// <summary>
        /// مبلغ الضريبة (TVA)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// نسبة الخصم على الفاتورة (%)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercentage { get; set; } = 0;

        /// <summary>
        /// قيمة الخصم بالدج
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        /// <summary>
        /// المجموع الكلي بعد الضريبة والخصم
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // ═══════════════════════════════════════════════════
        // 💳 معلومات الدفع
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// نوع الدفع: نقدي / كريدي / جزئي
        /// </summary>
        public PaymentType PaymentType { get; set; } = PaymentType.Cash;

        /// <summary>
        /// المبلغ المدفوع نقداً
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; } = 0;

        /// <summary>
        /// المبلغ المتبقي ديناً
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingAmount { get; set; } = 0;

        // ═══════════════════════════════════════════════════
        // 📊 حالة الفاتورة
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// حالة الفاتورة: مسودة / مكتملة / ملغاة
        /// </summary>
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Received; // البيع يكتمل فوراً عادة

        /// <summary>
        /// سبب الإلغاء (إذا تم الإلغاء)
        /// </summary>
        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        /// <summary>
        /// تاريخ الإلغاء
        /// </summary>
        public DateTime? CancelledAt { get; set; }

        // ═══════════════════════════════════════════════════
        // 💰 الربح (للتحليل المالي)
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// إجمالي تكلفة المنتجات المباعة
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        /// <summary>
        /// إجمالي الربح (TotalAmount - TotalCost - Discount)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalProfit { get; set; }

        // ═══════════════════════════════════════════════════
        // 👥 معلومات المستخدمين
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// المستخدم الذي أنشأ الفاتورة (البائع)
        /// </summary>
        public string? CreatedById { get; set; }

        [MaxLength(200)]
        public string? CreatedByName { get; set; }

        // ═══════════════════════════════════════════════════
        // 📝 ملاحظات وتواريخ
        // ═══════════════════════════════════════════════════

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // ═══════════════════════════════════════════════════
        // 🔗 Navigation Properties
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// بنود الفاتورة
        /// </summary>
        public ICollection<SaleItem> SaleItems { get; set; }
            = new List<SaleItem>();

        // ═══════════════════════════════════════════════════
        // 🧮 Computed Properties
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// هل الفاتورة مدفوعة بالكامل؟
        /// </summary>
        [NotMapped]
        public bool IsFullyPaid => RemainingAmount <= 0;

        /// <summary>
        /// نسبة الدفع
        /// </summary>
        [NotMapped]
        public decimal PaymentPercentage
            => TotalAmount > 0 ? Math.Round((PaidAmount / TotalAmount) * 100, 2) : 0;

        /// <summary>
        /// عدد البنود
        /// </summary>
        [NotMapped]
        public int ItemsCount => SaleItems?.Count ?? 0;

        /// <summary>
        /// إجمالي الكميات المباعة
        /// </summary>
        [NotMapped]
        public int TotalQuantity
            => SaleItems?.Sum(i => i.Quantity) ?? 0;

        /// <summary>
        /// نسبة الربح (%)
        /// </summary>
        [NotMapped]
        public decimal ProfitPercentage
            => TotalCost > 0 ? Math.Round((TotalProfit / TotalCost) * 100, 2) : 0;
    }
}