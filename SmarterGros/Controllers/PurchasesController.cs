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
    /// 🛒 Controller المشتريات - متكامل مع الصلاحيات و Activity Log
    /// </summary>
    [Authorize]
    public class PurchasesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPurchaseService _purchaseService;
        private readonly IActivityLogService _activityLogService;

        public PurchasesController(
            ApplicationDbContext context,
            IPurchaseService purchaseService,
            IActivityLogService activityLogService)
        {
            _context = context;
            _purchaseService = purchaseService;
            _activityLogService = activityLogService;
        }

        // ═══════════════════════════════════════════════════
        // 📋 صفحة القائمة الرئيسية
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Purchases.View)]
        public async Task<IActionResult> Index(
            string? search,
            int? supplierId,
            InvoiceStatus? status,
            PaymentType? paymentType,
            DateTime? dateFrom,
            DateTime? dateTo,
            string viewMode = "grid",
            int page = 1)
        {
            var model = await _purchaseService.GetPurchasesAsync(
                search, supplierId, status, paymentType, dateFrom, dateTo, page, 20);

            model.ViewMode = viewMode;

            // تسجيل المشاهدة
            await _activityLogService.LogViewAsync(
                module: "Purchases",
                description: "عرض قائمة المشتريات");

            return View(model);
        }

        // ═══════════════════════════════════════════════════
        // 👁️ صفحة التفاصيل
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Purchases.View)]
        public async Task<IActionResult> Details(int id)
        {
            var details = await _purchaseService.GetPurchaseDetailsAsync(id);

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
        [HasPermission(Permissions.Purchases.Create)]
        public async Task<IActionResult> Create()
        {
            await PrepareDropdowns();

            var model = new PurchaseCreateViewModel
            {
                PurchaseDate = DateTime.Now,
                PaymentType = PaymentType.Cash,
                SaveAsDraft = true,
                InvoiceNumber = await _purchaseService.GenerateInvoiceNumberAsync()
            };

            return View(model);
        }

        // ═══════════════════════════════════════════════════
        // 🆕 إنشاء فاتورة - حفظ
        // ═══════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.Purchases.Create)]
        public async Task<IActionResult> Create([FromBody] PurchaseCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(" | ", errors) });
            }

            var (success, message, purchaseId) = await _purchaseService.CreatePurchaseAsync(model);

            if (success)
            {
                return Json(new
                {
                    success = true,
                    message,
                    purchaseId,
                    redirectUrl = Url.Action(nameof(Details), new { id = purchaseId })
                });
            }

            return Json(new { success = false, message });
        }

        // ═══════════════════════════════════════════════════
        // ✏️ تعديل فاتورة - عرض
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Purchases.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _purchaseService.GetPurchaseForEditAsync(id);

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
        [HasPermission(Permissions.Purchases.Edit)]
        public async Task<IActionResult> Edit(int id, [FromBody] PurchaseCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(" | ", errors) });
            }

            var (success, message) = await _purchaseService.UpdatePurchaseAsync(id, model);

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
        [HasPermission(Permissions.Purchases.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _purchaseService.DeletePurchaseAsync(id);
            return Json(new { success, message });
        }

        // ═══════════════════════════════════════════════════
        // 📦 استلام فاتورة - عرض
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Purchases.Receive)]
        public async Task<IActionResult> Receive(int id)
        {
            var model = await _purchaseService.GetPurchaseForReceiveAsync(id);

            if (model == null)
            {
                TempData["Error"] = "لا يمكن استلام هذه الفاتورة";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(model);
        }

        // ═══════════════════════════════════════════════════
        // 📦 استلام فاتورة - تأكيد
        // ═══════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.Purchases.Receive)]
        public async Task<IActionResult> Receive([FromBody] PurchaseReceiveViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(" | ", errors) });
            }

            var (success, message) = await _purchaseService.ReceivePurchaseAsync(model);

            return Json(new
            {
                success,
                message,
                redirectUrl = success ? Url.Action(nameof(Details), new { id = model.PurchaseId }) : null
            });
        }

        // ═══════════════════════════════════════════════════
        // ❌ إلغاء فاتورة
        // ═══════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.Purchases.Cancel)]
        public async Task<IActionResult> Cancel([FromBody] PurchaseCancelViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(" | ", errors) });
            }

            var (success, message) = await _purchaseService.CancelPurchaseAsync(model);

            return Json(new
            {
                success,
                message,
                redirectUrl = success ? Url.Action(nameof(Details), new { id = model.PurchaseId }) : null
            });
        }

        // ═══════════════════════════════════════════════════
        // 📋 نسخ فاتورة (Duplicate)
        // ═══════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.Purchases.Duplicate)]
        public async Task<IActionResult> Duplicate(int id)
        {
            var (success, message, newId) = await _purchaseService.DuplicatePurchaseAsync(id);

            return Json(new
            {
                success,
                message,
                newPurchaseId = newId,
                redirectUrl = success ? Url.Action(nameof(Edit), new { id = newId }) : null
            });
        }

        // ═══════════════════════════════════════════════════
        // 💳 تسجيل دفعة
        // ═══════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.Purchases.ManagePayments)]
        public async Task<IActionResult> RegisterPayment([FromBody] SupplierPaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(" | ", errors) });
            }

            var (success, message) = await _purchaseService.RegisterPaymentAsync(model);
            return Json(new { success, message });
        }

        // ═══════════════════════════════════════════════════
        // 🖨️ طباعة الفاتورة
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Purchases.Print)]
        public async Task<IActionResult> Print(int id)
        {
            var details = await _purchaseService.GetPurchaseDetailsAsync(id);

            if (details == null)
            {
                TempData["Error"] = "الفاتورة غير موجودة";
                return RedirectToAction(nameof(Index));
            }

            // تسجيل عملية الطباعة
            await _activityLogService.LogAsync(
                actionType: "Print",
                actionName: "طباعة فاتورة شراء",
                module: "Purchases",
                entityName: "Purchase",
                entityId: id,
                description: $"طباعة الفاتورة {details.Purchase.InvoiceNumber}");

            return View("Print", details);
        }

        // ═══════════════════════════════════════════════════
        // 🔍 API: البحث عن منتج (للـ Modal)
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Purchases.Create)]
        public async Task<IActionResult> SearchProducts(string? term)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive);

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
                    purchasePrice = p.PurchasePriceHT,
                    taxRate = p.TaxRate,
                    stockQuantity = p.StockQuantity,
                    categoryName = p.Category != null ? p.Category.Name : ""
                })
                .ToListAsync();

            return Json(products);
        }

        // ═══════════════════════════════════════════════════
        // 🔍 API: البحث بالباركود
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Purchases.Create)]
        public async Task<IActionResult> SearchByBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return Json(new { success = false, message = "الباركود فارغ" });

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Barcode == barcode && p.IsActive);

            if (product == null)
                return Json(new { success = false, message = "المنتج غير موجود" });

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
                    purchasePrice = product.PurchasePriceHT,
                    taxRate = product.TaxRate,
                    stockQuantity = product.StockQuantity,
                    categoryName = product.Category?.Name ?? ""
                }
            });
        }

        // ═══════════════════════════════════════════════════
        // 🔍 API: الحصول على معلومات فاتورة (Quick Info)
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.Purchases.View)]
        public async Task<IActionResult> GetInvoiceInfo(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null)
                return Json(new { success = false, message = "الفاتورة غير موجودة" });

            return Json(new
            {
                success = true,
                data = new
                {
                    id = purchase.Id,
                    invoiceNumber = purchase.InvoiceNumber,
                    supplierName = purchase.Supplier?.Name,
                    purchaseDate = purchase.PurchaseDate.ToString("yyyy-MM-dd"),
                    status = purchase.Status.GetArabicName(),
                    statusColor = purchase.Status.GetBadgeColor(),
                    paymentType = purchase.PaymentType.GetArabicName(),
                    totalAmount = purchase.TotalAmount,
                    paidAmount = purchase.PaidAmount,
                    remainingAmount = purchase.RemainingAmount,
                    shippingCost = purchase.ShippingCost,
                    grandTotal = purchase.GrandTotal,
                    itemsCount = purchase.PurchaseItems.Count,
                    canEdit = purchase.Status.CanEdit(),
                    canDelete = purchase.Status.CanDelete(),
                    canReceive = purchase.Status.CanReceive(),
                    canCancel = purchase.Status.CanCancel(),
                    canReturn = purchase.Status.CanReturn()
                }
            });
        }

        // ═══════════════════════════════════════════════════
        // 🛠️ Helpers
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// تجهيز Dropdowns للـ Views (Suppliers, Products)
        /// </summary>
        private async Task PrepareDropdowns()
        {
            ViewBag.Suppliers = await _context.Suppliers
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();

            ViewBag.Products = await _context.Products
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();

            // ✅ جديد: Categories
            ViewBag.Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();


            // Enums للـ Dropdowns
            ViewBag.PaymentTypes = Enum.GetValues<PaymentType>()
                .Select(p => new SelectListItem
                {
                    Value = ((int)p).ToString(),
                    Text = p.GetArabicName()
                })
                .ToList();

            ViewBag.ShippingStatuses = Enum.GetValues<ShippingStatus>()
                .Select(s => new SelectListItem
                {
                    Value = ((int)s).ToString(),
                    Text = s.GetArabicName()
                })
                .ToList();
        }
    }
}


//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using SmarterGros.Data;
//using SmarterGros.Models;
//using SmarterGros.Models.Enums;

//namespace SmarterGros.Controllers
//{
//    [Authorize]
//    public class PurchasesController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public PurchasesController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<IActionResult> Index()
//        {
//            var purchases = await _context.Purchases
//                .Include(p => p.Supplier)
//                .Include(p => p.PurchaseItems)
//                .OrderByDescending(p => p.PurchaseDate)
//                .ToListAsync();
//            ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
//            ViewBag.Products = await _context.Products.Where(p => p.IsActive).Include(p => p.Category).ToListAsync();
//            return View(purchases);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Create([FromBody] PurchaseCreateViewModel model)
//        {
//            var lastPurchase = await _context.Purchases.OrderByDescending(p => p.Id).FirstOrDefaultAsync();
//            int nextId = (lastPurchase?.Id ?? 0) + 1;

//            var purchase = new Purchase
//            {
//                InvoiceNumber = $"FACT-{nextId:D6}",
//                SupplierId = model.SupplierId,
//                PurchaseDate = model.PurchaseDate,
//                Discount = model.Discount,
//                Notes = model.Notes,
//                Status = InvoiceStatus.Received
//            };

//            decimal subTotal = 0;
//            var items = new List<PurchaseItem>();

//            foreach (var item in model.Items)
//            {
//                var product = await _context.Products.FindAsync(item.ProductId);
//                if (product == null) continue;

//                var total = item.Quantity * item.UnitPrice;
//                subTotal += total;

//                items.Add(new PurchaseItem
//                {
//                    ProductId = item.ProductId,
//                    Quantity = item.Quantity,
//                    UnitPrice = item.UnitPrice,
//                    TotalPrice = total
//                });

//                var oldQty = product.StockQuantity;
//                product.StockQuantity += item.Quantity;
//                product.PurchasePriceTTC = item.UnitPrice;

//                _context.StockMovements.Add(new StockMovement
//                {
//                    ProductId = item.ProductId,
//                    MovementType = "إدخال",
//                    Quantity = item.Quantity,
//                    QuantityBefore = oldQty,
//                    QuantityAfter = product.StockQuantity,
//                    Reason = $"فاتورة شراء {purchase.InvoiceNumber}",
//                    UserName = User.Identity?.Name,
//                    MovementDate = DateTime.Now
//                });
//            }

//            purchase.SubTotal = subTotal;
//            purchase.TaxAmount = 0;
//            purchase.TotalAmount = subTotal - model.Discount;
//            purchase.PurchaseItems = items;

//            _context.Purchases.Add(purchase);
//            await _context.SaveChangesAsync();

//            return Json(new { success = true, invoiceNumber = purchase.InvoiceNumber });
//        }

//        [HttpGet]
//        public async Task<IActionResult> Details(int id)
//        {
//            var purchase = await _context.Purchases
//                .Include(p => p.Supplier)
//                .Include(p => p.PurchaseItems).ThenInclude(i => i.Product)
//                .FirstOrDefaultAsync(p => p.Id == id);
//            if (purchase == null) return NotFound();
//            return Json(purchase);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Delete(int id)
//        {
//            var purchase = await _context.Purchases
//                .Include(p => p.PurchaseItems)
//                .FirstOrDefaultAsync(p => p.Id == id);
//            if (purchase == null) return NotFound();

//            foreach (var item in purchase.PurchaseItems)
//            {
//                var product = await _context.Products.FindAsync(item.ProductId);
//                if (product != null)
//                {
//                    var oldQty = product.StockQuantity;
//                    product.StockQuantity -= item.Quantity;
//                    if (product.StockQuantity < 0) product.StockQuantity = 0;

//                    _context.StockMovements.Add(new StockMovement
//                    {
//                        ProductId = item.ProductId,
//                        MovementType = "إخراج",
//                        Quantity = -item.Quantity,
//                        QuantityBefore = oldQty,
//                        QuantityAfter = product.StockQuantity,
//                        Reason = $"إلغاء فاتورة شراء {purchase.InvoiceNumber}",
//                        UserName = User.Identity?.Name,
//                        MovementDate = DateTime.Now
//                    });
//                }
//            }

//            _context.Purchases.Remove(purchase);
//            await _context.SaveChangesAsync();
//            return Json(new { success = true });
//        }
//    }

//    public class PurchaseCreateViewModel
//    {
//        public int SupplierId { get; set; }
//        public DateTime PurchaseDate { get; set; }
//        public decimal Discount { get; set; }
//        public string? Notes { get; set; }
//        public List<PurchaseItemViewModel> Items { get; set; } = new();
//    }

//    public class PurchaseItemViewModel
//    {
//        public int ProductId { get; set; }
//        public int Quantity { get; set; }
//        public decimal UnitPrice { get; set; }
//    }
//}