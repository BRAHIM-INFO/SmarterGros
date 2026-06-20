using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmarterGros.Models
{
    /// <summary>
    /// 🔒 الجرد اليومي للصندوق
    /// يُسجَّل في نهاية كل يوم لمقارنة الرصيد المتوقع مع الفعلي
    /// </summary>
    public class DailyClosure
    {
        // ═══════════════════════════════════════════════════
        // 🔑 المفتاح الأساسي
        // ═══════════════════════════════════════════════════
        public int Id { get; set; }

        // ═══════════════════════════════════════════════════
        // 🔗 العلاقة مع الصندوق
        // ═══════════════════════════════════════════════════

        public int CashRegisterId { get; set; }
        public CashRegister? CashRegister { get; set; }

        // ═══════════════════════════════════════════════════
        // 📅 التاريخ
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// تاريخ اليوم (بدون وقت)
        /// </summary>
        [Column(TypeName = "date")]
        public DateTime ClosureDate { get; set; } = DateTime.Today;

        // ═══════════════════════════════════════════════════
        // 💰 الأرصدة
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// الرصيد الافتتاحي لليوم (في الصباح)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal OpeningBalance { get; set; }

        /// <summary>
        /// إجمالي الواردات في اليوم
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalIncome { get; set; }

        /// <summary>
        /// إجمالي الصادرات في اليوم
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalExpense { get; set; }

        /// <summary>
        /// الرصيد المتوقع في نهاية اليوم
        /// = OpeningBalance + TotalIncome - TotalExpense
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal ExpectedBalance { get; set; }

        /// <summary>
        /// الرصيد الفعلي (بعد العد اليدوي)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal ActualBalance { get; set; }

        /// <summary>
        /// الفرق (الفعلي - المتوقع)
        /// موجب = زيادة، سالب = نقص
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Difference { get; set; }

        // ═══════════════════════════════════════════════════
        // 💵 تفصيل النقود (Cash Denomination)
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// عدد فئة 2000 دج
        /// </summary>
        public int Count2000 { get; set; } = 0;

        /// <summary>
        /// عدد فئة 1000 دج
        /// </summary>
        public int Count1000 { get; set; } = 0;

        /// <summary>
        /// عدد فئة 500 دج
        /// </summary>
        public int Count500 { get; set; } = 0;

        /// <summary>
        /// عدد فئة 200 دج
        /// </summary>
        public int Count200 { get; set; } = 0;

        /// <summary>
        /// عدد فئة 100 دج
        /// </summary>
        public int Count100 { get; set; } = 0;

        /// <summary>
        /// عدد فئة 50 دج
        /// </summary>
        public int Count50 { get; set; } = 0;

        /// <summary>
        /// عدد فئة 20 دج
        /// </summary>
        public int Count20 { get; set; } = 0;

        /// <summary>
        /// عدد فئة 10 دج
        /// </summary>
        public int Count10 { get; set; } = 0;

        /// <summary>
        /// عدد فئة 5 دج
        /// </summary>
        public int Count5 { get; set; } = 0;

        /// <summary>
        /// عدد العملات المعدنية (مجموع)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal CoinsAmount { get; set; } = 0;

        // ═══════════════════════════════════════════════════
        // 📊 إحصائيات الحركات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// عدد الحركات في اليوم
        /// </summary>
        public int TransactionsCount { get; set; }

        /// <summary>
        /// عدد الواردات
        /// </summary>
        public int IncomeCount { get; set; }

        /// <summary>
        /// عدد الصادرات
        /// </summary>
        public int ExpenseCount { get; set; }

        // ═══════════════════════════════════════════════════
        // 📝 الملاحظات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// ملاحظات (خاصة إذا كان هناك فرق)
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// سبب الفرق (إن وُجد)
        /// </summary>
        [MaxLength(500)]
        public string? DifferenceReason { get; set; }

        // ═══════════════════════════════════════════════════
        // 🔒 معلومات الإغلاق
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// هل تم إغلاق اليوم نهائياً؟
        /// بعد الإغلاق، لا يمكن إضافة حركات لهذا اليوم
        /// </summary>
        public bool IsClosed { get; set; } = false;

        /// <summary>
        /// تاريخ ووقت الإغلاق
        /// </summary>
        public DateTime? ClosedAt { get; set; }

        /// <summary>
        /// المستخدم الذي أغلق اليوم
        /// </summary>
        public string? ClosedById { get; set; }

        [MaxLength(200)]
        public string? ClosedByName { get; set; }

        // ═══════════════════════════════════════════════════
        // 📅 التواريخ
        // ═══════════════════════════════════════════════════

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // ═══════════════════════════════════════════════════
        // 🔗 Navigation Properties
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// الحركات المضمنة في هذا الجرد
        /// </summary>
        public ICollection<CashTransaction> Transactions { get; set; }
            = new List<CashTransaction>();

        // ═══════════════════════════════════════════════════
        // 🧮 Computed Properties
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// إجمالي قيمة العد اليدوي
        /// </summary>
        [NotMapped]
        public decimal CashCountTotal
            => (Count2000 * 2000m) + (Count1000 * 1000m) + (Count500 * 500m)
             + (Count200 * 200m) + (Count100 * 100m) + (Count50 * 50m)
             + (Count20 * 20m) + (Count10 * 10m) + (Count5 * 5m)
             + CoinsAmount;

        /// <summary>
        /// نسبة الفرق (للتحليل)
        /// </summary>
        [NotMapped]
        public decimal DifferencePercentage
            => ExpectedBalance > 0
               ? Math.Round((Difference / ExpectedBalance) * 100, 2)
               : 0;

        /// <summary>
        /// هل هناك فرق؟
        /// </summary>
        [NotMapped]
        public bool HasDifference => Math.Abs(Difference) > 0.01m;

        /// <summary>
        /// نوع الفرق (زيادة/نقص/متطابق)
        /// </summary>
        [NotMapped]
        public string DifferenceType
        {
            get
            {
                if (!HasDifference) return "متطابق";
                return Difference > 0 ? "زيادة" : "نقص";
            }
        }

        /// <summary>
        /// لون الفرق للعرض
        /// </summary>
        [NotMapped]
        public string DifferenceColor
        {
            get
            {
                if (!HasDifference) return "success";
                return Difference > 0 ? "info" : "danger";
            }
        }
    }
}