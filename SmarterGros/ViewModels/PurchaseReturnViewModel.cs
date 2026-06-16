using SmarterGros.Models;
using SmarterGros.Models.Enums;

namespace SmarterGros.ViewModels
{
    /// <summary>
    /// 🔄 ViewModel لقائمة المرتجعات
    /// </summary>
    public class PurchaseReturnListViewModel
    {
        /// <summary>
        /// قائمة المرتجعات
        /// </summary>
        public List<PurchaseReturn> Returns { get; set; } = new();

        /// <summary>
        /// قائمة الموردين (للفلتر)
        /// </summary>
        public List<Supplier> Suppliers { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // 🔍 الفلاتر
        // ═══════════════════════════════════════════════════

        public string? SearchTerm { get; set; }
        public int? SupplierId { get; set; }
        public ReturnRefundMethod? RefundMethod { get; set; }
        public bool? IsCancelled { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        // ═══════════════════════════════════════════════════
        // 📊 الإحصائيات
        // ═══════════════════════════════════════════════════

        public int TotalCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalDeductedFromDebt { get; set; }
        public decimal TotalCashRefunded { get; set; }
        public int CancelledCount { get; set; }

        // ═══════════════════════════════════════════════════
        // 📄 Pagination
        // ═══════════════════════════════════════════════════

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// 🔄 ViewModel لعرض تفاصيل المرتجع
    /// </summary>
    public class PurchaseReturnDetailsViewModel
    {
        public PurchaseReturn PurchaseReturn { get; set; } = null!;
        public Purchase OriginalPurchase { get; set; } = null!;
        public List<ActivityLogViewModel> ActivityLogs { get; set; } = new();
    }
}