using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmarterGros.Data;
using SmarterGros.Security;
using SmarterGros.ViewModels;

namespace SmarterGros.Controllers
{
    [Authorize]
    [HasPermission(Permissions.Dashboard.View)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
            var totalStockValue = await _context.Products.Where(p => p.IsActive)
                .SumAsync(p => p.StockQuantity * p.PurchasePriceTTC);
            var totalSuppliers = await _context.Suppliers.CountAsync(s => s.IsActive);
            var totalCustomers = await _context.Customers.CountAsync(c => c.IsActive);
            var totalSales = await _context.Sales.SumAsync(s => s.TotalAmount);
            var totalPurchases = await _context.Purchases.SumAsync(p => p.TotalAmount);
            var lowStockProducts = await _context.Products
                .Where(p => p.IsActive && p.StockQuantity <= p.MinStockAlert)
                .Include(p => p.Category)
                .Take(10)
                .ToListAsync();
            var recentMovements = await _context.StockMovements
                .Include(m => m.Product)
                .OrderByDescending(m => m.MovementDate)
                .Take(5)
                .ToListAsync();
            var recentSales = await _context.Sales
                .Include(s => s.Customer)
                .OrderByDescending(s => s.SaleDate)
                .Take(5)
                .ToListAsync();

            var weeklyMovements = new List<int>();
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Now.Date.AddDays(-i);
                var count = await _context.StockMovements
                    .Where(m => m.MovementDate.Date == date)
                    .SumAsync(m => m.Quantity);
                weeklyMovements.Add(Math.Abs(count));
            }

            var vm = new DashboardViewModel
            {
                TotalProducts = totalProducts,
                TotalStockValue = totalStockValue,
                TotalSuppliers = totalSuppliers,
                TotalCustomers = totalCustomers,
                TotalSales = totalSales,
                TotalPurchases = totalPurchases,
                LowStockProducts = lowStockProducts,
                RecentMovements = recentMovements,
                WeeklyMovements = weeklyMovements,
                RecentSales = recentSales
            };

            return View(vm);
        }
    }
}