using SmarterGros.Models;
using SmarterGros.Models.Enums;

namespace SmarterGros.ViewModels.Reports
{
    /// <summary>
    /// 🛒 ViewModel لتقرير المشتريات الشامل
    /// </summary>
    public class PurchasesReportViewModel
    {
        // الفلاتر
        public DateTime DateFrom { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime DateTo { get; set; } = DateTime.Today;
        public int? SupplierId { get; set; }
        public int? CategoryId { get; set; }
        public PaymentType? PaymentType { get; set; }
        public string PeriodLabel { get; set; } = string.Empty;

        // الملخص
        public decimal TotalPurchases { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalDebt { get; set; }
        public decimal TotalShippingCost { get; set; }
        public int TotalInvoices { get; set; }
        public int TotalItemsPurchased { get; set; }
        public decimal AverageInvoiceValue { get; set; }

        // المقارنة
        public decimal PreviousPeriodPurchases { get; set; }
        public decimal PurchasesGrowthPercent { get; set; }

        // الرسوم البيانية
        public List<DailyPurchasesData> DailyPurchases { get; set; } = new();
        public List<CategoryPurchasesData> PurchasesByCategory { get; set; } = new();
        public List<PaymentTypeData> PurchasesByPaymentType { get; set; } = new();

        // الأعلى
        public List<TopSupplierData> TopSuppliers { get; set; } = new();
        public List<TopPurchasedProductData> TopPurchasedProducts { get; set; } = new();

        // الفواتير
        public List<Purchase> Invoices { get; set; } = new();

        // Dropdowns
        public List<Supplier> Suppliers { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
    }

    public class DailyPurchasesData
    {
        public DateTime Date { get; set; }
        public string DateLabel => Date.ToString("dd/MM");
        public decimal PurchasesAmount { get; set; }
        public int InvoicesCount { get; set; }
    }

    public class CategoryPurchasesData
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Quantity { get; set; }
        public decimal Percentage { get; set; }
        public string Color { get; set; } = "#667eea";
    }

    public class TopSupplierData
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int InvoicesCount { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal CurrentDebt { get; set; }
    }

    public class TopPurchasedProductData
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int QuantityPurchased { get; set; }
        public decimal TotalCost { get; set; }
        public int CurrentStock { get; set; }
    }
}