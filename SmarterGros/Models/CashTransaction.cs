using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmarterGros.Models.Enums;

namespace SmarterGros.Models
{
    /// <summary>
    /// 🔄 حركة الصندوق (واردة / صادرة)
    /// يمثل كل حركة مالية تمر عبر الصندوق
    /// </summary>
    public class CashTransaction
    {
        // ═══════════════════════════════════════════════════
        // 🔑 المفتاح الأساسي
        // ═══════════════════════════════════════════════════
        public int Id { get; set; }

        // ═══════════════════════════════════════════════════
        // 📋 معلومات الحركة الأساسية
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// رقم الحركة (مثال: TRX-2026-00001)
        /// يُولَّد تلقائياً
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string TransactionNumber { get; set; } = string.Empty;

        /// <summary>
        /// تاريخ الحركة
        /// </summary>
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        // ═══════════════════════════════════════════════════
        // 🔗 العلاقة مع الصندوق
        // ═══════════════════════════════════════════════════

        public int CashRegisterId { get; set; }
        public CashRegister? CashRegister { get; set; }

        // ═══════════════════════════════════════════════════
        // 💰 نوع وفئة الحركة
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// نوع الحركة: وارد أو صادر
        /// </summary>
        public TransactionType Type { get; set; }

        /// <summary>
        /// فئة الحركة (مبيعات، مشتريات، كهرباء...)
        /// </summary>
        public TransactionCategory Category { get; set; }

        /// <summary>
        /// طريقة الدفع
        /// </summary>
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        // ═══════════════════════════════════════════════════
        // 💵 المبلغ
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// مبلغ الحركة (موجب دائماً)
        /// النوع (Income/Expense) يحدد إن كان دخل أم خرج
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// رصيد الصندوق قبل الحركة
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceBefore { get; set; }

        /// <summary>
        /// رصيد الصندوق بعد الحركة
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAfter { get; set; }

        // ═══════════════════════════════════════════════════
        // 📝 الوصف والملاحظات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// وصف الحركة (إجباري)
        /// مثل: "دفع لمورد محمد فاتورة #001"
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// ملاحظات إضافية
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // ═══════════════════════════════════════════════════
        // 🔗 المرجع (ربط مع فاتورة أو عملية)
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// نوع المرجع: Purchase / Sale / PurchaseReturn / Manual...
        /// </summary>
        [MaxLength(50)]
        public string? ReferenceType { get; set; }

        /// <summary>
        /// رقم المرجع (ID الفاتورة المرتبطة)
        /// </summary>
        public int? ReferenceId { get; set; }

        /// <summary>
        /// رقم الفاتورة/المرجع للعرض السريع
        /// </summary>
        [MaxLength(50)]
        public string? ReferenceNumber { get; set; }

        // ═══════════════════════════════════════════════════
        // 🚚 معلومات الطرف المقابل (المورد/العميل)
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// ID المورد (إن كانت الحركة تخص مورد)
        /// </summary>
        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        /// <summary>
        /// ID العميل (إن كانت الحركة تخص عميل)
        /// </summary>
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        // ═══════════════════════════════════════════════════
        // 📜 الشيك / التحويل (إن وُجد)
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// رقم الشيك (إذا الدفع بشيك)
        /// </summary>
        [MaxLength(50)]
        public string? CheckNumber { get; set; }

        /// <summary>
        /// اسم البنك (للشيك أو التحويل)
        /// </summary>
        [MaxLength(100)]
        public string? BankName { get; set; }

        /// <summary>
        /// تاريخ استحقاق الشيك
        /// </summary>
        public DateTime? CheckDueDate { get; set; }

        // ═══════════════════════════════════════════════════
        // 👤 المسؤول عن الحركة
        // ═══════════════════════════════════════════════════

        public string? CreatedById { get; set; }

        [MaxLength(200)]
        public string? CreatedByName { get; set; }

        // ═══════════════════════════════════════════════════
        // 📅 التواريخ
        // ═══════════════════════════════════════════════════

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // ═══════════════════════════════════════════════════
        // ❌ الإلغاء
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// هل الحركة ملغاة؟
        /// لا نحذف الحركات، فقط نلغيها (للأمان والتدقيق)
        /// </summary>
        public bool IsCancelled { get; set; } = false;

        /// <summary>
        /// سبب الإلغاء
        /// </summary>
        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        public DateTime? CancelledAt { get; set; }
        public string? CancelledById { get; set; }

        [MaxLength(200)]
        public string? CancelledByName { get; set; }

        // ═══════════════════════════════════════════════════
        // 🔒 ربط بالجرد اليومي
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// إذا كانت الحركة تم تضمينها في جرد يوم محدد
        /// </summary>
        public int? DailyClosureId { get; set; }
        public DailyClosure? DailyClosure { get; set; }

        // ═══════════════════════════════════════════════════
        // 🧮 Computed Properties
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// المبلغ مع الإشارة (+/-)
        /// </summary>
        [NotMapped]
        public decimal SignedAmount
            => Type == TransactionType.Income ? Amount : -Amount;

        /// <summary>
        /// عرض المبلغ مع الإشارة (مثل: "+5,000" أو "-3,000")
        /// </summary>
        [NotMapped]
        public string DisplayAmount
            => $"{Type.GetSign()}{Amount:N2}";
    }
}