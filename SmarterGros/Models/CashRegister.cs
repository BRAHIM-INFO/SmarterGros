using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmarterGros.Models
{
    /// <summary>
    /// 💰 الصندوق (Caisse)
    /// يمثل صندوق النقد الرئيسي للمؤسسة
    /// يمكن أن يكون هناك أكثر من صندوق (مستقبلاً)
    /// </summary>
    public class CashRegister
    {
        // ═══════════════════════════════════════════════════
        // 🔑 المفتاح الأساسي
        // ═══════════════════════════════════════════════════
        public int Id { get; set; }

        // ═══════════════════════════════════════════════════
        // 📋 معلومات الصندوق
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// اسم الصندوق (مثل: الصندوق الرئيسي / صندوق الفرع 1)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "الصندوق الرئيسي";

        /// <summary>
        /// وصف الصندوق (اختياري)
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        // ═══════════════════════════════════════════════════
        // 💰 الأرصدة
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// الرصيد الافتتاحي - عند بدء استخدام النظام
        /// يُحدد مرة واحدة فقط
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal OpeningBalance { get; set; } = 0;

        /// <summary>
        /// الرصيد الحالي - يتحدث تلقائياً مع كل حركة
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; set; } = 0;

        /// <summary>
        /// تاريخ تحديد الرصيد الافتتاحي
        /// </summary>
        public DateTime? OpeningBalanceDate { get; set; }

        // ═══════════════════════════════════════════════════
        // 🎨 الحالة والإعدادات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// هل الصندوق نشط؟
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// هل هو الصندوق الافتراضي؟
        /// (يستخدم تلقائياً عند الحركات الآلية)
        /// </summary>
        public bool IsDefault { get; set; } = true;

        /// <summary>
        /// لون مميز للصندوق (للعرض البصري)
        /// </summary>
        [MaxLength(20)]
        public string? Color { get; set; } = "#28a745";

        /// <summary>
        /// أيقونة الصندوق
        /// </summary>
        [MaxLength(50)]
        public string? Icon { get; set; } = "fa-cash-register";

        // ═══════════════════════════════════════════════════
        // 📅 التواريخ
        // ═══════════════════════════════════════════════════

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // ═══════════════════════════════════════════════════
        // 👤 المسؤول
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// المستخدم المسؤول عن الصندوق (اختياري)
        /// </summary>
        public string? ResponsibleUserId { get; set; }

        [MaxLength(200)]
        public string? ResponsibleUserName { get; set; }

        // ═══════════════════════════════════════════════════
        // 🔗 Navigation Properties
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// كل الحركات على هذا الصندوق
        /// </summary>
        public ICollection<CashTransaction> Transactions { get; set; }
            = new List<CashTransaction>();

        /// <summary>
        /// الجردات اليومية لهذا الصندوق
        /// </summary>
        public ICollection<DailyClosure> DailyClosures { get; set; }
            = new List<DailyClosure>();

        // ═══════════════════════════════════════════════════
        // 🧮 Computed Properties (محسوبة)
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// إجمالي الواردات
        /// </summary>
        [NotMapped]
        public decimal TotalIncome
            => Transactions?
                .Where(t => t.Type == Enums.TransactionType.Income && !t.IsCancelled)
                .Sum(t => t.Amount) ?? 0;

        /// <summary>
        /// إجمالي الصادرات
        /// </summary>
        [NotMapped]
        public decimal TotalExpense
            => Transactions?
                .Where(t => t.Type == Enums.TransactionType.Expense && !t.IsCancelled)
                .Sum(t => t.Amount) ?? 0;

        /// <summary>
        /// الصافي (واردات - صادرات)
        /// </summary>
        [NotMapped]
        public decimal NetAmount => TotalIncome - TotalExpense;

        /// <summary>
        /// عدد الحركات
        /// </summary>
        [NotMapped]
        public int TransactionsCount => Transactions?.Count ?? 0;
    }
}