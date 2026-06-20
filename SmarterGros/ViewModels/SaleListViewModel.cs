using SmarterGros.Models;
using SmarterGros.Models.Enums;

namespace SmarterGros.ViewModels
{
    /// <summary>
    /// 📋 ViewModel لصفحة قائمة المبيعات
    /// </summary>
    public class SaleListViewModel
    {
        // ═══════════════════════════════════════════════════
        // 📊 البيانات الرئيسية
        // ═══════════════════════════════════════════════════

        public List<Sale> Sales { get; set; } = new();
        public List<Customer> Customers { get; set; } = new();
        public SaleStatsViewModel Stats { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // 🔍 الفلاتر
        // ═══════════════════════════════════════════════════

        public string? SearchTerm { get; set; }
        public int? CustomerId { get; set; }
        public InvoiceStatus? Status { get; set; }
        public PaymentType? PaymentType { get; set; }
        public SalePriceType? PriceType { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string ViewMode { get; set; } = "grid";

        // ═══════════════════════════════════════════════════
        // 📄 Pagination
        // ═══════════════════════════════════════════════════

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }
}