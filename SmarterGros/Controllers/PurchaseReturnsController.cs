using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmarterGros.Data;
using SmarterGros.Models.Enums;
using SmarterGros.Security;
using SmarterGros.Services;
using SmarterGros.ViewModels;

namespace SmarterGros.Controllers
{
    /// <summary>
    /// 🔄 Controller مرتجعات المشتريات
    /// </summary>
    [Authorize]
    public class PurchaseReturnsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPurchaseReturnService _returnService;
        private readonly IActivityLogService _activityLogService;

        public PurchaseReturnsController(
            ApplicationDbContext context,
            IPurchaseReturnService returnService,
            IActivityLogService activityLogService)
        {
            _context = context;
            _returnService = returnService;
            _activityLogService = activityLogService;
        }

        // ═══════════════════════════════════════════════════
        // 📋 صفحة القائمة
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.PurchaseReturns.View)]
        public async Task<IActionResult> Index(
            string? search,
            int? supplierId,
            ReturnRefundMethod? refundMethod,
            bool? isCancelled,
            DateTime? dateFrom,
            DateTime? dateTo,
            int page = 1)
        {
            var model = await _returnService.GetReturnsAsync(
                search, supplierId, refundMethod, isCancelled, dateFrom, dateTo, page, 20);

            await _activityLogService.LogViewAsync(
                module: "PurchaseReturns",
                description: "عرض قائمة مرتجعات المشتريات");

            return View(model);
        }

        // ═══════════════════════════════════════════════════
        // 👁️ صفحة التفاصيل
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.PurchaseReturns.View)]
        public async Task<IActionResult> Details(int id)
        {
            var details = await _returnService.GetReturnDetailsAsync(id);

            if (details == null)
            {
                TempData["Error"] = "المرتجع غير موجود";
                return RedirectToAction(nameof(Index));
            }

            return View(details);
        }

        // ═══════════════════════════════════════════════════
        // 🆕 إنشاء مرتجع - من فاتورة محددة
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.PurchaseReturns.Create)]
        public async Task<IActionResult> Create(int purchaseId)
        {
            // التحقق من إمكانية إنشاء مرتجع
            var (canReturn, reason) = await _returnService.CanCreateReturnForPurchaseAsync(purchaseId);

            if (!canReturn)
            {
                TempData["Error"] = reason;
                return RedirectToAction("Details", "Purchases", new { id = purchaseId });
            }

            var model = await _returnService.GetReturnFormForPurchaseAsync(purchaseId);

            if (model == null)
            {
                TempData["Error"] = "لا يمكن إنشاء مرتجع لهذه الفاتورة";
                return RedirectToAction("Index", "Purchases");
            }

            return View(model);
        }

        // ═══════════════════════════════════════════════════
        // 🆕 إنشاء مرتجع - حفظ
        // ═══════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.PurchaseReturns.Create)]
        public async Task<IActionResult> Create([FromBody] PurchaseReturnCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(" | ", errors) });
            }

            var (success, message, returnId) = await _returnService.CreateReturnAsync(model);

            return Json(new
            {
                success,
                message,
                returnId,
                redirectUrl = success ? Url.Action(nameof(Details), new { id = returnId }) : null
            });
        }

        // ═══════════════════════════════════════════════════
        // ❌ إلغاء مرتجع
        // ═══════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.PurchaseReturns.Cancel)]
        public async Task<IActionResult> Cancel(int id, string cancellationReason)
        {
            if (string.IsNullOrWhiteSpace(cancellationReason))
                return Json(new { success = false, message = "سبب الإلغاء مطلوب" });

            var (success, message) = await _returnService.CancelReturnAsync(id, cancellationReason);

            return Json(new
            {
                success,
                message,
                redirectUrl = success ? Url.Action(nameof(Details), new { id }) : null
            });
        }

        // ═══════════════════════════════════════════════════
        // 🗑️ حذف مرتجع (للأدمن - الملغى فقط)
        // ═══════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.PurchaseReturns.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _returnService.DeleteReturnAsync(id);
            return Json(new { success, message });
        }

        // ═══════════════════════════════════════════════════
        // 🖨️ طباعة مرتجع
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.PurchaseReturns.Print)]
        public async Task<IActionResult> Print(int id)
        {
            var details = await _returnService.GetReturnDetailsAsync(id);

            if (details == null)
            {
                TempData["Error"] = "المرتجع غير موجود";
                return RedirectToAction(nameof(Index));
            }

            // ✅ جلب إعدادات الشركة
            ViewBag.CompanySettings = await _context.CompanySettings.FirstOrDefaultAsync();

            await _activityLogService.LogAsync(
                actionType: "Print",
                actionName: "طباعة مرتجع شراء",
                module: "PurchaseReturns",
                entityName: "PurchaseReturn",
                entityId: id,
                description: $"طباعة المرتجع {details.PurchaseReturn.ReturnNumber}");

            return View("Print", details);
        }

        // ═══════════════════════════════════════════════════
        // 🔍 API: التحقق من إمكانية إنشاء مرتجع لفاتورة
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.PurchaseReturns.View)]
        public async Task<IActionResult> CheckCanReturn(int purchaseId)
        {
            var (canReturn, reason) = await _returnService.CanCreateReturnForPurchaseAsync(purchaseId);
            return Json(new { canReturn, reason });
        }

        // ═══════════════════════════════════════════════════
        // 🔍 API: الحصول على معلومات سريعة عن مرتجع
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.PurchaseReturns.View)]
        public async Task<IActionResult> GetReturnInfo(int id)
        {
            var purchaseReturn = await _context.PurchaseReturns
                .Include(r => r.Supplier)
                .Include(r => r.Purchase)
                .Include(r => r.ReturnItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (purchaseReturn == null)
                return Json(new { success = false, message = "المرتجع غير موجود" });

            return Json(new
            {
                success = true,
                data = new
                {
                    id = purchaseReturn.Id,
                    returnNumber = purchaseReturn.ReturnNumber,
                    returnDate = purchaseReturn.ReturnDate.ToString("yyyy-MM-dd"),
                    supplierName = purchaseReturn.Supplier?.Name,
                    purchaseInvoice = purchaseReturn.Purchase?.InvoiceNumber,
                    totalAmount = purchaseReturn.TotalAmount,
                    refundMethod = purchaseReturn.RefundMethod.GetArabicName(),
                    refundColor = purchaseReturn.RefundMethod.GetBadgeColor(),
                    deductedFromDebt = purchaseReturn.DeductedFromDebt,
                    cashRefunded = purchaseReturn.CashRefunded,
                    isCancelled = purchaseReturn.IsCancelled,
                    itemsCount = purchaseReturn.ReturnItems.Count,
                    canCancel = !purchaseReturn.IsCancelled
                }
            });
        }

        // ═══════════════════════════════════════════════════
        // 🔍 API: قائمة الفواتير القابلة للإرجاع لمورد
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.PurchaseReturns.Create)]
        public async Task<IActionResult> GetReturnablePurchases(int? supplierId)
        {
            var query = _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                .Where(p => p.Status == InvoiceStatus.Received);

            if (supplierId.HasValue)
                query = query.Where(p => p.SupplierId == supplierId.Value);

            var purchases = await query
                .OrderByDescending(p => p.PurchaseDate)
                .Take(50)
                .Select(p => new
                {
                    id = p.Id,
                    invoiceNumber = p.InvoiceNumber,
                    supplierName = p.Supplier != null ? p.Supplier.Name : "",
                    purchaseDate = p.PurchaseDate.ToString("yyyy-MM-dd"),
                    totalAmount = p.TotalAmount,
                    hasReturnableItems = p.PurchaseItems.Any(i => i.ReceivedQuantity > i.ReturnedQuantity)
                })
                .Where(p => p.hasReturnableItems)
                .ToListAsync();

            return Json(purchases);
        }
    }
}