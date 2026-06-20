using SmarterGros.Models;

namespace SmarterGros.ViewModels.Reports
{
    /// <summary>
    /// 📦 ViewModel لتقرير المخزون الشامل
    /// </summary>
    public class InventoryReportViewModel
    {
        // الفلاتر
        public int? CategoryId { get; set; }
        public string StockFilter { get; set; } = "all"; // all, low, out, expiring
        public string SortBy { get; set; } = "name"; // name, stock, value

        // الملخص
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int ExpiringProducts { get; set; }
        public int ExpiredProducts { get; set; }

        public decimal TotalInventoryValue { get; set; }
        public decimal TotalInventoryValueAtPurchase { get; set; }
        public decimal TotalInventoryValueAtRetail { get; set; }
        public decimal PotentialProfit { get; set; }

        // التوزيع حسب الفئة
        public List<CategoryInventoryData> CategoryDistribution { get; set; } = new();

        // المنتجات
        public List<ProductInventoryData> Products { get; set; } = new();

        // الأعلى/الأدنى
        public List<ProductInventoryData> HighestValueProducts { get; set; } = new();
        public List<ProductInventoryData> MostMovingProducts { get; set; } = new();
        public List<ProductInventoryData> SlowestMovingProducts { get; set; } = new();

        // Dropdowns
        public List<Category> Categories { get; set; } = new();
    }

    public class CategoryInventoryData
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ProductsCount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
        public decimal Percentage { get; set; }
        public string Color { get; set; } = "#667eea";
    }

    public class ProductInventoryData
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public int MinStockAlert { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal TotalValueAtCost { get; set; }
        public decimal TotalValueAtRetail { get; set; }
        public decimal PotentialProfit { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string StockStatus { get; set; } = "ok"; // ok, low, out, expiring, expired
        public int QuantitySoldLast30Days { get; set; }
    }
}