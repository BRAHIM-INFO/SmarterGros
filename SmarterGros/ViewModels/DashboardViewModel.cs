using SmarterGros.Models;

namespace SmarterGros.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public decimal TotalStockValue { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalCustomers { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalPurchases { get; set; }
        public List<Product> LowStockProducts { get; set; } = new();
        public List<StockMovement> RecentMovements { get; set; } = new();
        public List<Sale> RecentSales { get; set; } = new();
        public List<int> WeeklyMovements { get; set; } = new();
    }
}