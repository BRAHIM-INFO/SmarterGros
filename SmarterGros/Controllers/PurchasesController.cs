using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmarterGros.Data;
using SmarterGros.Models;
using SmarterGros.Models.Enums;

namespace SmarterGros.Controllers
{
    [Authorize]
    public class PurchasesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PurchasesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var purchases = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
            ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
            ViewBag.Products = await _context.Products.Where(p => p.IsActive).Include(p => p.Category).ToListAsync();
            return View(purchases);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PurchaseCreateViewModel model)
        {
            var lastPurchase = await _context.Purchases.OrderByDescending(p => p.Id).FirstOrDefaultAsync();
            int nextId = (lastPurchase?.Id ?? 0) + 1;

            var purchase = new Purchase
            {
                InvoiceNumber = $"PUR-{nextId:D6}",
                SupplierId = model.SupplierId,
                PurchaseDate = model.PurchaseDate,
                Discount = model.Discount,
                Notes = model.Notes,
                Status = InvoiceStatus.Received
            };

            decimal subTotal = 0;
            var items = new List<PurchaseItem>();

            foreach (var item in model.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null) continue;

                var total = item.Quantity * item.UnitPrice;
                subTotal += total;

                items.Add(new PurchaseItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = total
                });

                var oldQty = product.StockQuantity;
                product.StockQuantity += item.Quantity;
                product.PurchasePriceTTC = item.UnitPrice;

                _context.StockMovements.Add(new StockMovement
                {
                    ProductId = item.ProductId,
                    MovementType = "إدخال",
                    Quantity = item.Quantity,
                    QuantityBefore = oldQty,
                    QuantityAfter = product.StockQuantity,
                    Reason = $"فاتورة شراء {purchase.InvoiceNumber}",
                    UserName = User.Identity?.Name,
                    MovementDate = DateTime.Now
                });
            }

            purchase.SubTotal = subTotal;
            purchase.TaxAmount = 0;
            purchase.TotalAmount = subTotal - model.Discount;
            purchase.PurchaseItems = items;

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            return Json(new { success = true, invoiceNumber = purchase.InvoiceNumber });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (purchase == null) return NotFound();
            return Json(purchase);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.PurchaseItems)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (purchase == null) return NotFound();

            foreach (var item in purchase.PurchaseItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    var oldQty = product.StockQuantity;
                    product.StockQuantity -= item.Quantity;
                    if (product.StockQuantity < 0) product.StockQuantity = 0;

                    _context.StockMovements.Add(new StockMovement
                    {
                        ProductId = item.ProductId,
                        MovementType = "إخراج",
                        Quantity = -item.Quantity,
                        QuantityBefore = oldQty,
                        QuantityAfter = product.StockQuantity,
                        Reason = $"إلغاء فاتورة شراء {purchase.InvoiceNumber}",
                        UserName = User.Identity?.Name,
                        MovementDate = DateTime.Now
                    });
                }
            }

            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }

    public class PurchaseCreateViewModel
    {
        public int SupplierId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal Discount { get; set; }
        public string? Notes { get; set; }
        public List<PurchaseItemViewModel> Items { get; set; } = new();
    }

    public class PurchaseItemViewModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}