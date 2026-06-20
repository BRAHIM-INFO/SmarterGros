using System.ComponentModel.DataAnnotations;
using SmarterGros.Models;

namespace SmarterGros.ViewModels
{
    /// <summary>
    /// 🔒 ViewModel للجرد اليومي
    /// </summary>
    public class DailyClosureViewModel
    {
        public int? Id { get; set; }

        [Required]
        public int CashRegisterId { get; set; }

        /// <summary>
        /// اسم الصندوق (للعرض)
        /// </summary>
        public string? CashRegisterName { get; set; }

        // ═══════════════════════════════════════════════════
        // 📅 التاريخ
        // ═══════════════════════════════════════════════════

        [Required]
        [Display(Name = "تاريخ الجرد")]
        [DataType(DataType.Date)]
        public DateTime ClosureDate { get; set; } = DateTime.Today;

        // ═══════════════════════════════════════════════════
        // 💰 الأرصدة (للعرض)
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// الرصيد الافتتاحي للصباح
        /// </summary>
        public decimal OpeningBalance { get; set; }

        /// <summary>
        /// إجمالي الواردات
        /// </summary>
        public decimal TotalIncome { get; set; }

        /// <summary>
        /// إجمالي الصادرات
        /// </summary>
        public decimal TotalExpense { get; set; }

        /// <summary>
        /// الرصيد المتوقع
        /// </summary>
        public decimal ExpectedBalance { get; set; }

        // ═══════════════════════════════════════════════════
        // 💵 تفصيل العد اليدوي
        // ═══════════════════════════════════════════════════

        [Display(Name = "فئة 2000 دج")]
        [Range(0, int.MaxValue)]
        public int Count2000 { get; set; } = 0;

        [Display(Name = "فئة 1000 دج")]
        [Range(0, int.MaxValue)]
        public int Count1000 { get; set; } = 0;

        [Display(Name = "فئة 500 دج")]
        [Range(0, int.MaxValue)]
        public int Count500 { get; set; } = 0;

        [Display(Name = "فئة 200 دج")]
        [Range(0, int.MaxValue)]
        public int Count200 { get; set; } = 0;

        [Display(Name = "فئة 100 دج")]
        [Range(0, int.MaxValue)]
        public int Count100 { get; set; } = 0;

        [Display(Name = "فئة 50 دج")]
        [Range(0, int.MaxValue)]
        public int Count50 { get; set; } = 0;

        [Display(Name = "فئة 20 دج")]
        [Range(0, int.MaxValue)]
        public int Count20 { get; set; } = 0;

        [Display(Name = "فئة 10 دج")]
        [Range(0, int.MaxValue)]
        public int Count10 { get; set; } = 0;

        [Display(Name = "فئة 5 دج")]
        [Range(0, int.MaxValue)]
        public int Count5 { get; set; } = 0;

        [Display(Name = "العملات المعدنية (مجموع)")]
        [Range(0, double.MaxValue)]
        public decimal CoinsAmount { get; set; } = 0;

        // ═══════════════════════════════════════════════════
        // 📝 الملاحظات
        // ═══════════════════════════════════════════════════

        [MaxLength(1000)]
        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        [MaxLength(500)]
        [Display(Name = "سبب الفرق")]
        public string? DifferenceReason { get; set; }

        // ═══════════════════════════════════════════════════
        // 🔒 خيارات
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// إغلاق نهائي لليوم (لا يمكن التعديل بعدها)
        /// </summary>
        [Display(Name = "إغلاق نهائي لليوم")]
        public bool CloseDay { get; set; } = true;

        // ═══════════════════════════════════════════════════
        // 🧮 Computed Properties
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// إجمالي العد اليدوي
        /// </summary>
        public decimal CashCountTotal
            => (Count2000 * 2000m) + (Count1000 * 1000m) + (Count500 * 500m)
             + (Count200 * 200m) + (Count100 * 100m) + (Count50 * 50m)
             + (Count20 * 20m) + (Count10 * 10m) + (Count5 * 5m)
             + CoinsAmount;

        /// <summary>
        /// الفرق
        /// </summary>
        public decimal Difference => CashCountTotal - ExpectedBalance;

        /// <summary>
        /// هل هناك فرق؟
        /// </summary>
        public bool HasDifference => Math.Abs(Difference) > 0.01m;
    }

    /// <summary>
    /// 📋 ViewModel لقائمة الجردات السابقة
    /// </summary>
    public class DailyClosureListViewModel
    {
        public List<DailyClosure> Closures { get; set; } = new();

        // الفلاتر
        public int? CashRegisterId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool? HasDifferenceOnly { get; set; }

        // الإحصائيات
        public int TotalCount { get; set; }
        public int ClosedCount { get; set; }
        public int WithDifferenceCount { get; set; }
        public decimal TotalDifferences { get; set; }
    }
}