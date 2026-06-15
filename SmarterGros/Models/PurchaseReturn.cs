using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmarterGros.Models.Enums;

namespace SmarterGros.Models
{
    /// <summary>
    /// 🔄 مرتجع فاتورة شراء (Avoir / Bon de Retour)
    /// يمثل عملية إرجاع منتجات لمورد معين
    /// </summary>
    public class PurchaseReturn
    {
        // ═══════════════════════════════════════════════════
        // 🔑 المفتاح الأساسي
        // ═══════════════════════════════════════════════════
        public int Id { get; set; }

        // ═══════════════════════════════════════════════════
        // 📋 معلومات المرتجع الأساسية
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// رقم المرتجع (مثال: RET-2025-00001)
        /// يُولَّد تلقائياً
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string ReturnNumber { get; set; } = string.Empty;

        /// <summary>
        /// تاريخ المرتجع
        /// </summary>
        public DateTime ReturnDate { get; set; } = DateTime.Now;

        // ═══════════════════════════════════════════════════
        // 🔗 العلاقات الأساسية
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// معرف الفاتورة الأصلية
        /// </summary>
        public int PurchaseId { get; set; }
        public Purchase? Purchase { get; set; }

        /// <summary>
        /// معرف المورد (لتسهيل الاستعلامات)
        /// </summary>
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        // ═══════════════════════════════════════════════════
        // 💰 المبالغ المالية
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// المجموع قبل الضريبة
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        /// <summary>
        /// مبلغ الضريبة (TVA)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// المجموع الكلي للمرتجع
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // ═══════════════════════════════════════════════════
        // 💳 طريقة الاسترداد
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// طريقة استرداد قيمة المرتجع
        /// خصم من الدين / نقدي / مزيج
        /// </summary>
        public ReturnRefundMethod RefundMethod { get; set; }
            = ReturnRefundMethod.DeductFromDebt;

        /// <summary>
        /// المبلغ الذي تم خصمه من الدين
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal DeductedFromDebt { get; set; } = 0;

        /// <summary>
        /// المبلغ الذي تم استرداده نقداً
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal CashRefunded { get; set; } = 0;

        // ═══════════════════════════════════════════════════
        // 📝 معلومات إضافية
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// سبب الإرجاع العام (للمرتجع كله)
        /// </summary>
        [MaxLength(500)]
        public string? ReturnReason { get; set; }

        /// <summary>
        /// ملاحظات
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // ═══════════════════════════════════════════════════
        // 👥 معلومات المستخدم
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// المستخدم الذي أنشأ المرتجع
        /// </summary>
        public string? CreatedById { get; set; }

        /// <summary>
        /// اسم المستخدم
        /// </summary>
        [MaxLength(200)]
        public string? CreatedByName { get; set; }

        // ═══════════════════════════════════════════════════
        // 📅 التواريخ
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// تاريخ الإنشاء
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// تاريخ آخر تعديل
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // ═══════════════════════════════════════════════════
        // ❌ الإلغاء
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// هل المرتجع ملغى؟
        /// </summary>
        public bool IsCancelled { get; set; } = false;

        /// <summary>
        /// سبب الإلغاء
        /// </summary>
        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        /// <summary>
        /// تاريخ الإلغاء
        /// </summary>
        public DateTime? CancelledAt { get; set; }

        // ═══════════════════════════════════════════════════
        // 🔗 Navigation Properties
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// بنود المرتجع
        /// </summary>
        public ICollection<PurchaseReturnItem> ReturnItems { get; set; }
            = new List<PurchaseReturnItem>();

        // ═══════════════════════════════════════════════════
        // 🧮 Computed Properties (محسوبة)
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// عدد البنود في المرتجع
        /// </summary>
        [NotMapped]
        public int ItemsCount => ReturnItems?.Count ?? 0;

        /// <summary>
        /// إجمالي الكميات المرتجعة
        /// </summary>
        [NotMapped]
        public int TotalQuantity
            => ReturnItems?.Sum(i => i.ReturnedQuantity) ?? 0;

        /// <summary>
        /// التحقق من صحة طريقة الاسترداد
        /// (المجموع المسترد = المجموع الكلي)
        /// </summary>
        [NotMapped]
        public bool IsRefundValid
            => (DeductedFromDebt + CashRefunded) == TotalAmount;

        /// <summary>
        /// المبلغ الكلي المسترد (للتحقق)
        /// </summary>
        [NotMapped]
        public decimal TotalRefunded
            => DeductedFromDebt + CashRefunded;
    }
}