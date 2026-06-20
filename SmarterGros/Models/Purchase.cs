using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmarterGros.Models.Enums;

namespace SmarterGros.Models
{
    /// <summary>
    /// 📄 فاتورة الشراء (Facture d'achat)
    /// تمثل عملية شراء كاملة من مورد معين
    /// </summary>
    public class Purchase
    {
        // ═══════════════════════════════════════════════════
        // 🔑 المفتاح الأساسي
        // ═══════════════════════════════════════════════════
        public int Id { get; set; }

        // ═══════════════════════════════════════════════════
        // 📋 معلومات الفاتورة الأساسية
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// رقم الفاتورة (مثال: FACT-2025-00001)
        /// يُولَّد تلقائياً
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        /// <summary>
        /// رقم الفاتورة الأصلية من المورد (إن وجد)
        /// مفيد للمطابقة مع الفاتورة الورقية
        /// </summary>
        [MaxLength(50)]
        public string? SupplierInvoiceNumber { get; set; }

        /// <summary>
        /// تاريخ إنشاء الفاتورة
        /// </summary>
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        // ═══════════════════════════════════════════════════
        // 🚚 معلومات المورد
        // ═══════════════════════════════════════════════════

        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        // ═══════════════════════════════════════════════════
        // 💰 المبالغ المالية
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
        /// المجموع الكلي بعد الضريبة والخصم (بدون الشحن)
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
        // 🚚 معلومات الشحن (Transporteur)
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// اسم مكتب الشحن أو السائق
        /// </summary>
        [MaxLength(200)]
        public string? TransporterName { get; set; }

        /// <summary>
        /// رقم هاتف الناقل
        /// </summary>
        [MaxLength(20)]
        public string? TransporterPhone { get; set; }

        /// <summary>
        /// رقم وصل الشحن (BL - Bon de Livraison)
        /// </summary>
        [MaxLength(50)]
        public string? DeliveryNoteNumber { get; set; }

        /// <summary>
        /// تكلفة الشحن (مصروف منفصل)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCost { get; set; } = 0;

        /// <summary>
        /// تاريخ الشحن
        /// </summary>
        public DateTime? ShippingDate { get; set; }

        /// <summary>
        /// تاريخ الاستلام الفعلي
        /// </summary>
        public DateTime? ReceivedDate { get; set; }

        /// <summary>
        /// حالة الشحن
        /// </summary>
        public ShippingStatus? ShippingStatus { get; set; }

        // ═══════════════════════════════════════════════════
        // 📊 حالة الفاتورة
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// حالة الفاتورة: مسودة / مرسلة / مستلمة / ملغاة
        /// </summary>
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

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
        // 👥 معلومات المستخدمين
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// المستخدم الذي أنشأ الفاتورة
        /// </summary>
        public string? CreatedById { get; set; }

        /// <summary>
        /// اسم المستخدم الذي أنشأ الفاتورة (للعرض السريع)
        /// </summary>
        [MaxLength(200)]
        public string? CreatedByName { get; set; }

        /// <summary>
        /// المستخدم الذي استلم الفاتورة
        /// </summary>
        public string? ReceivedById { get; set; }

        /// <summary>
        /// اسم المستخدم الذي استلم الفاتورة
        /// </summary>
        [MaxLength(200)]
        public string? ReceivedByName { get; set; }

        // ═══════════════════════════════════════════════════
        // 📝 ملاحظات وتواريخ
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// ملاحظات على الفاتورة
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// تاريخ إنشاء السجل
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// تاريخ آخر تعديل
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // ═══════════════════════════════════════════════════
        // 🔗 Navigation Properties (العلاقات)
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// بنود الفاتورة (المنتجات)
        /// </summary>
        public ICollection<PurchaseItem> PurchaseItems { get; set; }
            = new List<PurchaseItem>();

        /// <summary>
        /// المرتجعات المرتبطة بهذه الفاتورة
        /// </summary>
        public ICollection<PurchaseReturn> Returns { get; set; }
            = new List<PurchaseReturn>();

        // ═══════════════════════════════════════════════════
        // 🧮 Computed Properties (محسوبة - غير محفوظة في DB)
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// المجموع الكلي شامل الشحن
        /// </summary>
        [NotMapped]
        public decimal GrandTotal => TotalAmount + ShippingCost;

        /// <summary>
        /// هل الفاتورة مدفوعة بالكامل؟
        /// </summary>
        [NotMapped]
        public bool IsFullyPaid => RemainingAmount <= 0;

        /// <summary>
        /// نسبة الدفع (من 0 إلى 100)
        /// </summary>
        [NotMapped]
        public decimal PaymentPercentage
            => TotalAmount > 0 ? Math.Round((PaidAmount / TotalAmount) * 100, 2) : 0;

        /// <summary>
        /// عدد البنود في الفاتورة
        /// </summary>
        [NotMapped]
        public int ItemsCount => PurchaseItems?.Count ?? 0;

        /// <summary>
        /// إجمالي الكميات
        /// </summary>
        [NotMapped]
        public int TotalQuantity
            => PurchaseItems?.Sum(i => i.Quantity) ?? 0;

        /// <summary>
        /// هل لها مرتجعات؟
        /// </summary>
        [NotMapped]
        public bool HasReturns => Returns?.Any() ?? false;

        /// <summary>
        /// إجمالي قيمة المرتجعات
        /// </summary>
        [NotMapped]
        public decimal TotalReturnsAmount
            => Returns?.Sum(r => r.TotalAmount) ?? 0;

        /// <summary>
        /// الصافي بعد المرتجعات
        /// </summary>
        [NotMapped]
        public decimal NetAmount => GrandTotal - TotalReturnsAmount;
    }
}