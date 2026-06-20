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
    /// 💰 Controller الصندوق
    /// </summary>
    [Authorize]
    public class CashRegisterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICashRegisterService _cashService;
        private readonly IActivityLogService _activityLogService;

        public CashRegisterController(
            ApplicationDbContext context,
            ICashRegisterService cashService,
            IActivityLogService activityLogService)
        {
            _context = context;
            _cashService = cashService;
            _activityLogService = activityLogService;
        }

        // ═══════════════════════════════════════════════════
        // 🏠 الصفحة الرئيسية - Dashboard
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.CashRegister.View)]
        public async Task<IActionResult> Index(int? cashRegisterId)
        {
            var dashboard = await _cashService.GetDashboardAsync(cashRegisterId);

            // تسجيل المشاهدة
            await _activityLogService.LogViewAsync(
                module: "CashRegister",
                description: "عرض لوحة الصندوق");

            return View(dashboard);
        }

        // ═══════════════════════════════════════════════════
        // 📜 قائمة الحركات
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.CashRegister.View)]
        public async Task<IActionResult> Transactions(
            int? cashRegisterId,
            string? search,
            TransactionType? type,
            TransactionCategory? category,
            PaymentMethod? paymentMethod,
            int? supplierId,
            int? customerId,
            DateTime? dateFrom,
            DateTime? dateTo,
            bool? isCancelled,
            int page = 1)
        {
            var model = await _cashService.GetTransactionsAsync(
                cashRegisterId, search, type, category, paymentMethod,
                supplierId, customerId, dateFrom, dateTo, isCancelled,
                page, 50);

            return View(model);
        }

        // ═══════════════════════════════════════════════════
        // 👁️ تفاصيل حركة
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.CashRegister.View)]
        public async Task<IActionResult> TransactionDetails(int id)
        {
            var transaction = await _cashService.GetTransactionByIdAsync(id);

            if (transaction == null)
            {
                TempData["Error"] = "الحركة غير موجودة";
                return RedirectToAction(nameof(Transactions));
            }

            return View(transaction);
        }

        // ═══════════════════════════════════════════════════
        // ➕ إضافة حركة - عرض الصفحة
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.CashRegister.AddTransaction)]
        public async Task<IActionResult> AddTransaction(TransactionType? defaultType)
        {
            var register = await _cashService.GetDefaultRegisterAsync();

            if (register == null)
            {
                TempData["Error"] = "لا يوجد صندوق نشط";
                return RedirectToAction(nameof(Index));
            }

            var model = new CashTransactionViewModel
            {
                CashRegisterId = register.Id,
                TransactionDate = DateTime.Now,
                Type = defaultType ?? TransactionType.Expense,
                Category = defaultType == TransactionType.Income
                    ? TransactionCategory.OtherIncome
                    : TransactionCategory.OtherExpense,
                PaymentMethod = PaymentMethod.Cash
            };

            await PrepareAddTransactionDropdowns();

            ViewBag.CurrentBalance = register.CurrentBalance;
            ViewBag.RegisterName = register.Name;

            return View(model);
        }

        // ═══════════════════════════════════════════════════
        // ➕ إضافة حركة - حفظ
        // ═══════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.CashRegister.AddTransaction)]
        public async Task<IActionResult> AddTransaction([FromBody] CashTransactionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(" | ", errors) });
            }

            var (success, message, transactionId) = await _cashService.AddTransactionAsync(model);

            return Json(new
            {
                success,
                message,
                transactionId,
                redirectUrl = success ? Url.Action(nameof(Index)) : null
            });
        }

        // ═══════════════════════════════════════════════════
        // ❌ إلغاء حركة
        // ═══════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.CashRegister.CancelTransaction)]
        public async Task<IActionResult> CancelTransaction(int id, string cancellationReason)
        {
            if (string.IsNullOrWhiteSpace(cancellationReason))
                return Json(new { success = false, message = "سبب الإلغاء مطلوب" });

            var model = new CancelCashTransactionViewModel
            {
                TransactionId = id,
                CancellationReason = cancellationReason
            };

            var (success, message) = await _cashService.CancelTransactionAsync(model);

            return Json(new
            {
                success,
                message,
                redirectUrl = success ? Url.Action(nameof(Transactions)) : null
            });
        }

        // ═══════════════════════════════════════════════════
        // 🗑️ حذف حركة (للأدمن - الملغاة فقط)
        // ═══════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.CashRegister.DeleteTransaction)]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var (success, message) = await _cashService.DeleteTransactionAsync(id);
            return Json(new { success, message });
        }

        // ═══════════════════════════════════════════════════
        // 🔒 الجرد اليومي - عرض
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.CashRegister.CloseDaily)]
        public async Task<IActionResult> DailyClosure(int? cashRegisterId, DateTime? date)
        {
            try
            {
                var model = await _cashService.PrepareDailyClosureAsync(cashRegisterId, date);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // ═══════════════════════════════════════════════════
        // 🔒 الجرد اليومي - حفظ
        // ═══════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.CashRegister.CloseDaily)]
        public async Task<IActionResult> DailyClosure([FromBody] DailyClosureViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(" | ", errors) });
            }

            var (success, message, closureId) = await _cashService.PerformDailyClosureAsync(model);

            return Json(new
            {
                success,
                message,
                closureId,
                redirectUrl = success ? Url.Action(nameof(Index)) : null
            });
        }

        // ═══════════════════════════════════════════════════
        // 📋 قائمة الجردات
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.CashRegister.CloseDaily)]
        public async Task<IActionResult> Closures(
            int? cashRegisterId,
            DateTime? dateFrom,
            DateTime? dateTo,
            bool? hasDifferenceOnly)
        {
            var model = await _cashService.GetClosuresAsync(
                cashRegisterId, dateFrom, dateTo, hasDifferenceOnly);
            return View(model);
        }

        // ═══════════════════════════════════════════════════
        // 👁️ تفاصيل جرد
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.CashRegister.CloseDaily)]
        public async Task<IActionResult> ClosureDetails(int id)
        {
            var closure = await _cashService.GetClosureByIdAsync(id);

            if (closure == null)
            {
                TempData["Error"] = "الجرد غير موجود";
                return RedirectToAction(nameof(Closures));
            }

            return View(closure);
        }

        // ═══════════════════════════════════════════════════
        // 📊 التقارير
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.CashRegister.ViewReports)]
        public async Task<IActionResult> Reports(
            DateTime? dateFrom,
            DateTime? dateTo,
            int? cashRegisterId,
            string reportType = "monthly")
        {
            var from = dateFrom ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var to = dateTo ?? DateTime.Today;

            var report = await _cashService.GenerateReportAsync(from, to, cashRegisterId, reportType);
            return View(report);
        }

        // ═══════════════════════════════════════════════════
        // 📊 تقرير سريع - يومي
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.CashRegister.ViewReports)]
        public async Task<IActionResult> DailyReport(DateTime? date, int? cashRegisterId)
        {
            var report = await _cashService.GetDailyReportAsync(date ?? DateTime.Today, cashRegisterId);
            return View("Reports", report);
        }

        // ═══════════════════════════════════════════════════
        // 📊 تقرير شهري
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.CashRegister.ViewReports)]
        public async Task<IActionResult> MonthlyReport(int? year, int? month, int? cashRegisterId)
        {
            var report = await _cashService.GetMonthlyReportAsync(
                year ?? DateTime.Now.Year,
                month ?? DateTime.Now.Month,
                cashRegisterId);
            return View("Reports", report);
        }

        // ═══════════════════════════════════════════════════
        // 📊 تقرير سنوي
        // ═══════════════════════════════════════════════════

        [HttpGet]
        [HasPermission(Permissions.CashRegister.ViewReports)]
        public async Task<IActionResult> YearlyReport(int? year, int? cashRegisterId)
        {
            var report = await _cashService.GetYearlyReportAsync(
                year ?? DateTime.Now.Year,
                cashRegisterId);
            return View("Reports", report);
        }

        // ═══════════════════════════════════════════════════
        // ⚙️ إعدادات الصندوق - الرصيد الافتتاحي
        // ═══════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.CashRegister.SetOpeningBalance)]
        public async Task<IActionResult> SetOpeningBalance(int cashRegisterId, decimal openingBalance)
        {
            var (success, message) = await _cashService.SetOpeningBalanceAsync(cashRegisterId, openingBalance);
            return Json(new { success, message });
        }

        // ═══════════════════════════════════════════════════
        // 🔍 APIs مساعدة
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// API: الحصول على الفئات حسب النوع
        /// </summary>
        [HttpGet]
        [HasPermission(Permissions.CashRegister.View)]
        public IActionResult GetCategoriesByType(TransactionType type)
        {
            var categories = type == TransactionType.Income
                ? TransactionCategoryExtensions.GetIncomeCategories()
                : TransactionCategoryExtensions.GetExpenseCategories();

            var result = categories.Select(c => new
            {
                value = (int)c,
                name = c.GetArabicName(),
                icon = c.GetIcon(),
                color = c.GetColor()
            }).ToList();

            return Json(result);
        }

        /// <summary>
        /// API: الحصول على الرصيد الحالي
        /// </summary>
        [HttpGet]
        [HasPermission(Permissions.CashRegister.View)]
        public async Task<IActionResult> GetCurrentBalance(int? cashRegisterId)
        {
            var balance = await _cashService.GetCurrentBalanceAsync(cashRegisterId);
            return Json(new { balance });
        }

        /// <summary>
        /// API: التحقق من إغلاق اليوم
        /// </summary>
        [HttpGet]
        [HasPermission(Permissions.CashRegister.View)]
        public async Task<IActionResult> IsDayClosed(int cashRegisterId, DateTime date)
        {
            var isClosed = await _cashService.IsDayClosedAsync(cashRegisterId, date);
            return Json(new { isClosed });
        }

        // ═══════════════════════════════════════════════════
        // 🛠️ Helpers
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// تجهيز Dropdowns لصفحة إضافة الحركة
        /// </summary>
        private async Task PrepareAddTransactionDropdowns()
        {
            // الصناديق
            ViewBag.CashRegisters = await _context.CashRegisters
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();

            // الموردين
            ViewBag.Suppliers = await _context.Suppliers
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();

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

            // ✅ فئات الواردات - تحويل إلى List<object>
            ViewBag.IncomeCategories = TransactionCategoryExtensions.GetIncomeCategories()
                .Select(c => new
                {
                    value = (int)c,
                    name = c.GetArabicName(),
                    icon = c.GetIcon()
                })
                .Cast<object>()  // ⭐ مهم!
                .ToList();

            // ✅ فئات الصادرات - تحويل إلى List<object>
            ViewBag.ExpenseCategories = TransactionCategoryExtensions.GetExpenseCategories()
                .Select(c => new
                {
                    value = (int)c,
                    name = c.GetArabicName(),
                    icon = c.GetIcon()
                })
                .Cast<object>()  // ⭐ مهم!
                .ToList();

            // طرق الدفع
            ViewBag.PaymentMethods = Enum.GetValues<PaymentMethod>()
                .Select(p => new SelectListItem
                {
                    Value = ((int)p).ToString(),
                    Text = p.GetArabicName()
                })
                .ToList();
        }
    }
}