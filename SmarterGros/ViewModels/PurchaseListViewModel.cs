using SmarterGros.Models;
using SmarterGros.Models.Enums;

namespace SmarterGros.ViewModels
{
    /// <summary>
    /// 📋 ViewModel لصفحة قائمة المشتريات
    /// يحتوي على القائمة + الفلاتر + الإحصائيات
    /// </summary>
    public class PurchaseListViewModel
    {
        // ═══════════════════════════════════════════════════
        // 📊 البيانات الرئيسية
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// قائمة الفواتير
        /// </summary>
        public List<Purchase> Purchases { get; set; } = new();

        /// <summary>
        /// قائمة الموردين (للفلتر)
        /// </summary>
        public List<Supplier> Suppliers { get; set; } = new();

        /// <summary>
        /// الإحصائيات
        /// </summary>
        public PurchaseStatsViewModel Stats { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // 🔍 الفلاتر
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// نص البحث (رقم الفاتورة، اسم المورد، إلخ)
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// فلتر بالمورد
        /// </summary>
        public int? SupplierId { get; set; }

        /// <summary>
        /// فلتر بحالة الفاتورة
        /// </summary>
        public InvoiceStatus? Status { get; set; }

        /// <summary>
        /// فلتر بنوع الدفع
        /// </summary>
        public PaymentType? PaymentType { get; set; }

        /// <summary>
        /// فلتر من تاريخ
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// فلتر إلى تاريخ
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// طريقة العرض (شبكة / قائمة)
        /// </summary>
        public string ViewMode { get; set; } = "grid"; // grid or list

        // ═══════════════════════════════════════════════════
        // 📄 Pagination
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// الصفحة الحالية
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// عدد العناصر في الصفحة
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// إجمالي عدد الصفحات
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// إجمالي عدد الفواتير
        /// </summary>
        public int TotalCount { get; set; }
    }
}