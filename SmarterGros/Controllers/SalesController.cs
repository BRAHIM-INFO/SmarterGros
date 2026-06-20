using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmarterGros.Data;
using SmarterGros.Models;
using SmarterGros.Models.Enums;
using SmarterGros.Security;
using SmarterGros.Services;
using SmarterGros.ViewModels;

namespace SmarterGros.Controllers
{
    /// <summary>
    /// 💰 Controller المبيعات
    /// </summary>
    [Authorize]
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISaleService _saleService;
        private readonly IActivityLogService _activityLogService;

        public SalesController(
            ApplicationDbContext context,
            ISaleService saleService,
            IActivityLogService activityLogService)
        {
            _context = context;
            _saleService = saleService;
            _activityLogService = activityLogService;
        }

        // ═══════════════════════════════════════════════════
        // 📋 صفحة القائمة الرئيسية
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Sales.View)]
        public async Task<IActionResult> Index(
            string? search,
            int? customerId,
            InvoiceStatus? status,
            PaymentType? paymentType,
            SalePriceType? priceType,
            DateTime? dateFrom,
            DateTime? dateTo,
            string viewMode = "grid",
            int page = 1)
        {
            var model = await _saleService.GetSalesAsync(
                search, customerId, status, paymentType, priceType,
                dateFrom, dateTo, page, 20);

            model.ViewMode = viewMode;

            await _activityLogService.LogViewAsync(
                module: "Sales",
                description: "عرض قائمة المبيعات");

            return View(model);
        }

        // ═══════════════════════════════════════════════════
        // 👁️ صفحة التفاصيل
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Sales.View)]
        public async Task<IActionResult> Details(int id)
        {
            var details = await _saleService.GetSaleDetailsAsync(id);

            if (details == null)
            {
                TempData["Error"] = "الفاتورة غير موجودة";
                return RedirectToAction(nameof(Index));
            }

            return View(details);
        }

        // ═══════════════════════════════════════════════════
        // 🆕 إنشاء فاتورة - عرض الصفحة
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Sales.Create)]
        public async Task<IActionResult> Create()
        {
            await PrepareDropdowns();

            var model = new SaleCreateViewModel
            {
                SaleDate = DateTime.Now,
                PriceType = SalePriceType.Retail,
                PaymentType = PaymentType.Cash,
                SaveAsDraft = false,
                InvoiceNumber = await _saleService.GenerateInvoiceNumberAsync()
            };

            return View(model);
        }

        // ═══════════════════════════════════════════════════
        // 🆕 إنشاء فاتورة - حفظ
        // ═══════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.Sales.Create)]
        public async Task<IActionResult> Create([FromBody] SaleCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(" | ", errors) });
            }

            var (success, message, saleId) = await _saleService.CreateSaleAsync(model);

            if (success)
            {
                return Json(new
                {
                    success = true,
                    message,
                    saleId,
                    redirectUrl = Url.Action(nameof(Details), new { id = saleId })
                });
            }

            return Json(new { success = false, message });
        }

        // ═══════════════════════════════════════════════════
        // ✏️ تعديل فاتورة - عرض
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Sales.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _saleService.GetSaleForEditAsync(id);

            if (model == null)
            {
                TempData["Error"] = "الفاتورة غير موجودة أو لا يمكن تعديلها";
                return RedirectToAction(nameof(Index));
            }

            await PrepareDropdowns();
            return View(model);
        }

        // ═══════════════════════════════════════════════════
        // ✏️ تعديل فاتورة - حفظ
        // ═══════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.Sales.Edit)]
        public async Task<IActionResult> Edit(int id, [FromBody] SaleCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(" | ", errors) });
            }

            var (success, message) = await _saleService.UpdateSaleAsync(id, model);

            if (success)
            {
                return Json(new
                {
                    success = true,
                    message,
                    redirectUrl = Url.Action(nameof(Details), new { id })
                });
            }

            return Json(new { success = false, message });
        }

        // ═══════════════════════════════════════════════════
        // 🗑️ حذف فاتورة
        // ═══════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.Sales.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _saleService.DeleteSaleAsync(id);
            return Json(new { success, message });
        }

        // ═══════════════════════════════════════════════════
        // ❌ إلغاء فاتورة
        // ═══════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.Sales.Cancel)]
        public async Task<IActionResult> Cancel([FromBody] SaleCancelViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(" | ", errors) });
            }

            var (success, message) = await _saleService.CancelSaleAsync(model);

            return Json(new
            {
                success,
                message,
                redirectUrl = success ? Url.Action(nameof(Details), new { id = model.SaleId }) : null
            });
        }

        // ═══════════════════════════════════════════════════
        // 📋 نسخ فاتورة
        // ═══════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.Sales.Duplicate)]
        public async Task<IActionResult> Duplicate(int id)
        {
            var (success, message, newId) = await _saleService.DuplicateSaleAsync(id);

            return Json(new
            {
                success,
                message,
                newSaleId = newId,
                redirectUrl = success ? Url.Action(nameof(Edit), new { id = newId }) : null
            });
        }

        // ═══════════════════════════════════════════════════
        // 💳 تسجيل دفعة من عميل
        // ═══════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.Sales.ManagePayments)]
        public async Task<IActionResult> RegisterPayment([FromBody] CustomerPaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(" | ", errors) });
            }

            var (success, message) = await _saleService.RegisterPaymentAsync(model);
            return Json(new { success, message });
        }

        // ═══════════════════════════════════════════════════
        // 🖨️ طباعة الفاتورة
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Sales.Print)]
        public async Task<IActionResult> Print(int id)
        {
            var details = await _saleService.GetSaleDetailsAsync(id);

            if (details == null)
            {
                TempData["Error"] = "الفاتورة غير موجودة";
                return RedirectToAction(nameof(Index));
            }

            await _activityLogService.LogAsync(
                actionType: "Print",
                actionName: "طباعة فاتورة بيع",
                module: "Sales",
                entityName: "Sale",
                entityId: id,
                description: $"طباعة الفاتورة {details.Sale.InvoiceNumber}");

            return View("Print", details);
        }

        // ═══════════════════════════════════════════════════
        // 🔍 API: البحث عن منتج (للـ Modal)
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Sales.Create)]
        public async Task<IActionResult> SearchProducts(string? term, SalePriceType priceType = SalePriceType.Retail)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.StockQuantity > 0); // فقط المنتجات المتوفرة

            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(p =>
                    p.Name.Contains(term) ||
                    p.Reference.Contains(term) ||
                    (p.Barcode != null && p.Barcode.Contains(term)));
            }

            var products = await query
                .Take(20)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    reference = p.Reference,
                    barcode = p.Barcode,
                    unit = p.Unit,
                    purchasePrice = p.PurchasePriceTTC,
                    retailPrice = p.RetailPriceTTC,
                    semiWholesalePrice = p.SemiWholesalePriceTTC,
                    wholesalePrice = p.WholesalePriceTTC,
                    taxRate = p.TaxRate,
                    stockQuantity = p.StockQuantity,
                    minStockAlert = p.MinStockAlert,
                    categoryName = p.Category != null ? p.Category.Name : ""
                })
                .ToListAsync();

            return Json(products);
        }

        // ═══════════════════════════════════════════════════
        // 🔍 API: البحث بالباركود
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Sales.Create)]
        public async Task<IActionResult> SearchByBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return Json(new { success = false, message = "الباركود فارغ" });

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Barcode == barcode && p.IsActive);

            if (product == null)
                return Json(new { success = false, message = "المنتج غير موجود" });

            if (product.StockQuantity <= 0)
                return Json(new { success = false, message = $"المنتج '{product.Name}' غير متوفر في المخزون" });

            return Json(new
            {
                success = true,
                product = new
                {
                    id = product.Id,
                    name = product.Name,
                    reference = product.Reference,
                    barcode = product.Barcode,
                    unit = product.Unit,
                    purchasePrice = product.PurchasePriceTTC,
                    retailPrice = product.RetailPriceTTC,
                    semiWholesalePrice = product.SemiWholesalePriceTTC,
                    wholesalePrice = product.WholesalePriceTTC,
                    taxRate = product.TaxRate,
                    stockQuantity = product.StockQuantity,
                    categoryName = product.Category?.Name ?? ""
                }
            });
        }

        // ═══════════════════════════════════════════════════
        // 🔍 API: التحقق من توفر المخزون
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Sales.Create)]
        public async Task<IActionResult> CheckStock(int productId, int quantity)
        {
            var (available, message) = await _saleService.CheckStockAvailabilityAsync(productId, quantity);
            return Json(new { available, message });
        }

        // ═══════════════════════════════════════════════════
        // 🔍 API: الحصول على معلومات فاتورة
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Sales.View)]
        public async Task<IActionResult> GetSaleInfo(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
                return Json(new { success = false, message = "الفاتورة غير موجودة" });

            return Json(new
            {
                success = true,
                data = new
                {
                    id = sale.Id,
                    invoiceNumber = sale.InvoiceNumber,
                    customerName = sale.Customer?.Name ?? sale.CustomerName,
                    saleDate = sale.SaleDate.ToString("yyyy-MM-dd"),
                    status = sale.Status.GetArabicName(),
                    statusColor = sale.Status.GetBadgeColor(),
                    paymentType = sale.PaymentType.GetArabicName(),
                    priceType = sale.PriceType.GetArabicName(),
                    totalAmount = sale.TotalAmount,
                    paidAmount = sale.PaidAmount,
                    remainingAmount = sale.RemainingAmount,
                    totalProfit = sale.TotalProfit,
                    itemsCount = sale.SaleItems.Count,
                    canEdit = sale.Status == InvoiceStatus.Draft,
                    canDelete = sale.Status == InvoiceStatus.Draft,
                    canCancel = sale.Status != InvoiceStatus.Cancelled
                }
            });
        }


        // ═══════════════════════════════════════════════════
        // 🛒 نقطة البيع (POS)
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Sales.QuickSale)]
        public async Task<IActionResult> POS()
        {
            // الحصول على أكثر المنتجات مبيعاً (للأزرار السريعة)
            ViewBag.PopularProducts = await _context.Products
                .Where(p => p.IsActive && p.StockQuantity > 0)
                .OrderByDescending(p => p.SaleItems.Count)
                .Take(12)
                .ToListAsync();

            return View();
        }

        // ═══════════════════════════════════════════════════
        // 🛠️ Helpers
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// تجهيز Dropdowns للـ Views
        /// </summary>
        private async Task PrepareDropdowns()
        {
            // العملاء
            ViewBag.Customers = await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();

            // المنتجات (فقط المتوفرة)
            ViewBag.Products = await _context.Products
                .Where(p => p.IsActive && p.StockQuantity > 0)
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();

            // الفئات (لإضافة منتج سريع)
            ViewBag.Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            // أنواع الأسعار
            ViewBag.PriceTypes = Enum.GetValues<SalePriceType>()
                .Select(p => new SelectListItem
                {
                    Value = ((int)p).ToString(),
                    Text = p.GetArabicName()
                })
                .ToList();

            // أنواع الدفع
            ViewBag.PaymentTypes = Enum.GetValues<PaymentType>()
                .Select(p => new SelectListItem
                {
                    Value = ((int)p).ToString(),
                    Text = p.GetArabicName()
                })
                .ToList();
        }
    }
}