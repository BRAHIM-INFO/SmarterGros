using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using SmarterGros.Data;
using SmarterGros.Models;
using SmarterGros.Security;
using System.Drawing;
using System.Drawing.Imaging;
using SmarterGros.Services; // ✅ جديد

namespace SmarterGros.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IActivityLogService _activityLog; // ✅ جديد


        public ProductsController(
            ApplicationDbContext context,
            IWebHostEnvironment env,
            IActivityLogService activityLog) // ✅ جديد
        {
            _context = context;
            _env = env;
            _activityLog = activityLog; // ✅ جديد
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

        [HttpGet]
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

            // ✅ تسجيل العملية
            await _activityLog.LogCreateAsync(
                module: "Products",
                entityName: "Product",
                entityId: product.Id,
                description: $"تم إضافة منتج جديد: {product.Name} (المرجع: {product.Reference})",
                newValues: new
                {
                    product.Name,
                    product.Reference,
                    product.CategoryId,
                    product.StockQuantity,
                    product.PurchasePriceTTC,
                    product.RetailPriceTTC
                }
            );


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

        // ═══════════════════════════════════════════════════
        // ✏️ تعديل منتج
        // ═══════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
        {
            var existing = await _context.Products.FindAsync(id);
            if (existing == null)
                return NotFound();

            // تحديث البيانات
            existing.Name = product.Name;
            existing.Reference = product.Reference;
            existing.CategoryId = product.CategoryId;
            existing.Unit = product.Unit;
            existing.Description = product.Description;
            existing.PurchasePriceHT = product.PurchasePriceHT;
            existing.PurchasePriceTTC = product.PurchasePriceTTC;
            existing.TaxRate = product.TaxRate;
            existing.RetailPriceHT = product.RetailPriceHT;
            existing.RetailPriceTTC = product.RetailPriceTTC;
            existing.RetailMargin = product.RetailMargin;
            existing.SemiWholesalePriceHT = product.SemiWholesalePriceHT;
            existing.SemiWholesalePriceTTC = product.SemiWholesalePriceTTC;
            existing.SemiWholesaleMargin = product.SemiWholesaleMargin;
            existing.WholesalePriceHT = product.WholesalePriceHT;
            existing.WholesalePriceTTC = product.WholesalePriceTTC;
            existing.WholesaleMargin = product.WholesaleMargin;
            existing.StockQuantity = product.StockQuantity;
            existing.MinStockAlert = product.MinStockAlert;
            existing.ExpiryDate = product.ExpiryDate;
            existing.Location = product.Location;
            existing.Zone = product.Zone;
            existing.Aisle = product.Aisle;
            existing.Shelf = product.Shelf;
            existing.Level = product.Level;
            existing.Bin = product.Bin;

            // تحديث الصورة إذا وُجدت
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
                Directory.CreateDirectory(uploadsDir);

                var fileName = $"product_{id}_{DateTime.Now.Ticks}{Path.GetExtension(imageFile.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                existing.ImagePath = $"/uploads/products/{fileName}";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [HasPermission(Permissions.Products.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            // ✅ حفظ البيانات قبل الحذف
            var deletedData = new
            {
                product.Name,
                product.Reference,
                product.StockQuantity,
                product.PurchasePriceTTC,
                product.RetailPriceTTC
            };


            product.IsActive = false;
            await _context.SaveChangesAsync();

            // ✅ تسجيل الحذف (Critical تلقائياً)
            await _activityLog.LogDeleteAsync(
                module: "Products",
                entityName: "Product",
                entityId: id,
                description: $"تم حذف منتج: {product.Name} (المرجع: {product.Reference})",
                deletedData: deletedData
            ); 

            TempData["Success"] = "تم حذف المنتج بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ═══════════════════════════════════════════════════
        // 🔍 API: جلب بيانات منتج للتعديل
        // ═══════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return Json(new { success = false, message = "المنتج غير موجود" });

            return Json(new
            {
                success = true,
                id = product.Id,
                name = product.Name,
                reference = product.Reference,
                categoryId = product.CategoryId,
                barcode = product.Barcode,
                unit = product.Unit,
                description = product.Description,
                purchasePriceHT = product.PurchasePriceHT,
                purchasePriceTTC = product.PurchasePriceTTC,
                taxRate = product.TaxRate,
                retailPriceHT = product.RetailPriceHT,
                retailPriceTTC = product.RetailPriceTTC,
                retailMargin = product.RetailMargin,
                semiWholesalePriceHT = product.SemiWholesalePriceHT,
                semiWholesalePriceTTC = product.SemiWholesalePriceTTC,
                semiWholesaleMargin = product.SemiWholesaleMargin,
                wholesalePriceHT = product.WholesalePriceHT,
                wholesalePriceTTC = product.WholesalePriceTTC,
                wholesaleMargin = product.WholesaleMargin,
                stockQuantity = product.StockQuantity,
                minStockAlert = product.MinStockAlert,
                packagingQty = product.PackagingQty,
                expiryDate = product.ExpiryDate?.ToString("yyyy-MM-dd"),
                location = product.Location,
                zone = product.Zone,
                aisle = product.Aisle,
                shelf = product.Shelf,
                level = product.Level,
                bin = product.Bin,
                imagePath = product.ImagePath
            });
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


        // ═══════════════════════════════════════════════════
        // ⚡ API: إضافة منتج سريع
        // ═══════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.Products.Create)]
        public async Task<IActionResult> CreateQuick([FromBody] QuickProductViewModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Name))
                    return Json(new { success = false, message = "اسم المنتج مطلوب" });

                if (string.IsNullOrWhiteSpace(model.Reference))
                    return Json(new { success = false, message = "المرجع مطلوب" });

                // التحقق من تكرار المرجع
                var exists = await _context.Products
                    .AnyAsync(p => p.Reference == model.Reference.Trim());

                if (exists)
                    return Json(new { success = false, message = "يوجد منتج بنفس المرجع" });

                var product = new Product
                {
                    Name = model.Name.Trim(),
                    Reference = model.Reference.Trim(),
                    CategoryId = model.CategoryId,
                    Barcode = string.IsNullOrWhiteSpace(model.Barcode) ? null : model.Barcode.Trim(),
                    Unit = string.IsNullOrWhiteSpace(model.Unit) ? null : model.Unit.Trim(),
                    PurchasePriceHT = model.PurchasePriceHT,
                    TaxRate = model.TaxRate,
                    PurchasePriceTTC = model.PurchasePriceHT * (1 + model.TaxRate / 100),
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    StockQuantity = 0
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                await _activityLog.LogCreateAsync(
                    module: "Products",
                    entityName: "Product",
                    entityId: product.Id,
                    description: $"إضافة منتج سريع: {product.Name}",
                    newValues: new { product.Name, product.Reference, product.PurchasePriceHT });

                return Json(new
                {
                    success = true,
                    message = "تم إضافة المنتج بنجاح",
                    product = new
                    {
                        id = product.Id,
                        name = product.Name,
                        reference = product.Reference,
                        unit = product.Unit,
                        purchasePrice = product.PurchasePriceHT,
                        taxRate = product.TaxRate
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"حدث خطأ: {ex.Message}" });
            }
        }

        public class QuickProductViewModel
        {
            public string Name { get; set; } = string.Empty;
            public string Reference { get; set; } = string.Empty;
            public int CategoryId { get; set; }
            public string? Barcode { get; set; }
            public string? Unit { get; set; }
            public decimal PurchasePriceHT { get; set; }
            public decimal TaxRate { get; set; } = 0;
        }


    }
}