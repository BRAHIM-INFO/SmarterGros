using SmarterGros.Models;
using SmarterGros.Models.Enums;

namespace SmarterGros.ViewModels.Reports
{
    /// <summary>
    /// 💰 ViewModel لتقرير المبيعات الشامل
    /// </summary>
    public class SalesReportViewModel
    {
        // ═══════════════════════════════════════════════════
        // 🔍 الفلاتر
        // ═══════════════════════════════════════════════════
        public DateTime DateFrom { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime DateTo { get; set; } = DateTime.Today;
        public int? CustomerId { get; set; }
        public int? CategoryId { get; set; }
        public SalePriceType? PriceType { get; set; }
        public PaymentType? PaymentType { get; set; }
        public string PeriodLabel { get; set; } = string.Empty;

        // ═══════════════════════════════════════════════════
        // 📊 ملخص شامل
        // ═══════════════════════════════════════════════════
        public decimal TotalSales { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal TotalDebt { get; set; }
        public int TotalInvoices { get; set; }
        public int TotalItemsSold { get; set; }
        public decimal AverageInvoiceValue { get; set; }
        public decimal ProfitMargin { get; set; }

        // ═══════════════════════════════════════════════════
        // 📈 المقارنة مع الفترة السابقة
        // ═══════════════════════════════════════════════════
        public decimal PreviousPeriodSales { get; set; }
        public decimal PreviousPeriodProfit { get; set; }
        public decimal SalesGrowthPercent { get; set; }
        public decimal ProfitGrowthPercent { get; set; }

        // ═══════════════════════════════════════════════════
        // 📅 بيانات الرسوم البيانية
        // ═══════════════════════════════════════════════════
        public List<DailySalesData> DailySales { get; set; } = new();
        public List<CategorySalesData> SalesByCategory { get; set; } = new();
        public List<PaymentTypeData> SalesByPaymentType { get; set; } = new();
        public List<PriceTypeData> SalesByPriceType { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // 🏆 أعلى
        // ═══════════════════════════════════════════════════
        public List<TopProductData> TopProducts { get; set; } = new();
        public List<TopCustomerData> TopCustomers { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // 📋 الفواتير التفصيلية
        // ═══════════════════════════════════════════════════
        public List<Sale> Invoices { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // 🔽 Dropdowns
        // ═══════════════════════════════════════════════════
        public List<Customer> Customers { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
    }

    public class DailySalesData
    {
        public DateTime Date { get; set; }
        public string DateLabel => Date.ToString("dd/MM");
        public decimal SalesAmount { get; set; }
        public decimal Profit { get; set; }
        public int InvoicesCount { get; set; }
    }

    public class CategorySalesData
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Quantity { get; set; }
        public decimal Percentage { get; set; }
        public string Color { get; set; } = "#667eea";
    }

    public class PaymentTypeData
    {
        public string PaymentTypeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class PriceTypeData
    {
        public string PriceTypeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class TopProductData
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalProfit { get; set; }
    }

    public class TopCustomerData
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int InvoicesCount { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal CurrentDebt { get; set; }
    }
}