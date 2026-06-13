using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmarterGros.Data;

namespace SmarterGros.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ReportsController(ApplicationDbContext context) { _context = context; }

        public IActionResult Index() => View();

        public async Task<IActionResult> Stock()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> LowStock()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.StockQuantity <= p.MinStockAlert)
                .ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Sales(DateTime? from, DateTime? to)
        {
            var query = _context.Sales.Include(s => s.Customer).Include(s => s.SaleItems).AsQueryable();
            if (from.HasValue) query = query.Where(s => s.SaleDate >= from.Value);
            if (to.HasValue) query = query.Where(s => s.SaleDate <= to.Value.AddDays(1));
            var sales = await query.OrderByDescending(s => s.SaleDate).ToListAsync();
            ViewBag.From = from;
            ViewBag.To = to;
            return View(sales);
        }

        public async Task<IActionResult> Purchases(DateTime? from, DateTime? to)
        {
            var query = _context.Purchases.Include(p => p.Supplier).Include(p => p.PurchaseItems).AsQueryable();
            if (from.HasValue) query = query.Where(p => p.PurchaseDate >= from.Value);
            if (to.HasValue) query = query.Where(p => p.PurchaseDate <= to.Value.AddDays(1));
            var purchases = await query.OrderByDescending(p => p.PurchaseDate).ToListAsync();
            ViewBag.From = from;
            ViewBag.To = to;
            return View(purchases);
        }

        public async Task<IActionResult> Statistics()
        {
            var totalSales = await _context.Sales.SumAsync(s => s.TotalAmount);
            var totalPurchases = await _context.Purchases.SumAsync(p => p.TotalAmount);
            var totalProfit = await _context.SaleItems.SumAsync(i => i.Profit);
            var topProducts = await _context.SaleItems
                .Include(i => i.Product)
                .GroupBy(i => i.Product!.Name)
                .Select(g => new { Name = g.Key, Qty = g.Sum(i => i.Quantity), Total = g.Sum(i => i.TotalPrice) })
                .OrderByDescending(x => x.Qty)
                .Take(5)
                .ToListAsync();
            var topCustomers = await _context.Sales
                .Where(s => s.CustomerId != null)
                .Include(s => s.Customer)
                .GroupBy(s => s.Customer!.Name)
                .Select(g => new { Name = g.Key, Total = g.Sum(s => s.TotalAmount), Count = g.Count() })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalSales = totalSales;
            ViewBag.TotalPurchases = totalPurchases;
            ViewBag.TotalProfit = totalProfit;
            ViewBag.TopProducts = topProducts;
            ViewBag.TopCustomers = topCustomers;
            return View();
        }
    }
}