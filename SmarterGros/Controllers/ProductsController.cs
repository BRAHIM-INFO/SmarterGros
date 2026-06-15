using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using SmarterGros.Data;
using SmarterGros.Models;
using SmarterGros.Security;
using System.Drawing;
using System.Drawing.Imaging;

namespace SmarterGros.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HasPermission(Permissions.Products.View)]
        public async Task<IActionResult> Index(string? search, int? categoryId, string? view)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search) || p.Reference.Contains(search) || (p.Barcode != null && p.Barcode.Contains(search)));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            var products = await query.ToListAsync();
            var categories = await _context.Categories.ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.ViewMode = view ?? "grid";
            ViewBag.TotalStockValue = products.Sum(p => p.StockQuantity * p.PurchasePriceTTC);
            ViewBag.TotalRetailValue = products.Sum(p => p.StockQuantity * p.RetailPriceTTC);
            ViewBag.TotalWholesaleValue = products.Sum(p => p.StockQuantity * p.WholesalePriceTTC);
            ViewBag.TotalCost = products.Sum(p => p.StockQuantity * p.PurchasePriceTTC);
            ViewBag.TotalProfit = products.Sum(p => p.StockQuantity * (p.RetailPriceTTC - p.PurchasePriceTTC));

            return View(products);
        }

        [HttpPost]
        [HasPermission(Permissions.Products.Create)]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
            var lastRef = await _context.Products.OrderByDescending(p => p.Id).FirstOrDefaultAsync();
            int nextId = (lastRef?.Id ?? 0) + 1;
            ViewBag.NextReference = $"PRD-{nextId:D4}";
            return View();
        }

        [HttpPost]
        [HasPermission(Permissions.Products.Create)]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (imageFile != null)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "products");
                Directory.CreateDirectory(uploadsDir);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);
                product.ImagePath = $"/uploads/products/{fileName}";
            }

            product.QRCode = GenerateQRCodeBase64(product.Reference);
            product.PurchasePriceTTC = product.PurchasePriceHT * (1 + product.TaxRate / 100);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var movement = new StockMovement
            {
                ProductId = product.Id,
                MovementType = "إدخال",
                Quantity = product.StockQuantity,
                QuantityBefore = 0,
                QuantityAfter = product.StockQuantity,
                Reason = "إضافة منتج جديد",
                UserName = User.Identity?.Name,
                MovementDate = DateTime.Now
            };
            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إضافة المنتج بنجاح";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [HasPermission(Permissions.Products.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
            return View(product);
        }

        [HttpPost]
        [HasPermission(Permissions.Products.Edit)]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
        {
            var existing = await _context.Products.FindAsync(id);
            if (existing == null) return NotFound();

            var oldQty = existing.StockQuantity;

            existing.Name = product.Name;
            existing.CategoryId = product.CategoryId;
            existing.Barcode = product.Barcode;
            existing.Unit = product.Unit;
            existing.Location = product.Location;
            existing.Zone = product.Zone;
            existing.Aisle = product.Aisle;
            existing.Shelf = product.Shelf;
            existing.Level = product.Level;
            existing.Bin = product.Bin;
            existing.PurchasePriceHT = product.PurchasePriceHT;
            existing.TaxRate = product.TaxRate;
            existing.PurchasePriceTTC = product.PurchasePriceHT * (1 + product.TaxRate / 100);
            existing.WholesalePriceHT = product.WholesalePriceHT;
            existing.WholesalePriceTTC = product.WholesalePriceTTC;
            existing.WholesaleMargin = product.WholesaleMargin;
            existing.SemiWholesalePriceHT = product.SemiWholesalePriceHT;
            existing.SemiWholesalePriceTTC = product.SemiWholesalePriceTTC;
            existing.SemiWholesaleMargin = product.SemiWholesaleMargin;
            existing.RetailPriceHT = product.RetailPriceHT;
            existing.RetailPriceTTC = product.RetailPriceTTC;
            existing.RetailMargin = product.RetailMargin;
            existing.StockQuantity = product.StockQuantity;
            existing.MinStockAlert = product.MinStockAlert;
            existing.PackagingQty = product.PackagingQty;
            existing.ExpiryDate = product.ExpiryDate;
            existing.Description = product.Description;

            if (imageFile != null)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "products");
                Directory.CreateDirectory(uploadsDir);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);
                existing.ImagePath = $"/uploads/products/{fileName}";
            }

            if (oldQty != product.StockQuantity)
            {
                var movement = new StockMovement
                {
                    ProductId = id,
                    MovementType = "تعديل",
                    Quantity = product.StockQuantity - oldQty,
                    QuantityBefore = oldQty,
                    QuantityAfter = product.StockQuantity,
                    Reason = "تعديل يدوي",
                    UserName = User.Identity?.Name,
                    MovementDate = DateTime.Now
                };
                _context.StockMovements.Add(movement);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم تعديل المنتج بنجاح";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [HasPermission(Permissions.Products.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            product.IsActive = false;
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف المنتج بنجاح";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [HasPermission(Permissions.Products.View)]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return Json(product);
        }

        [HttpGet]
        public IActionResult GenerateReference()
        {
            var lastRef = _context.Products.OrderByDescending(p => p.Id).FirstOrDefault();
            int nextId = (lastRef?.Id ?? 0) + 1;
            return Json(new { reference = $"PRD-{nextId:D4}" });
        }

        private string GenerateQRCodeBase64(string content)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(10);
            return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
        }
    }
}