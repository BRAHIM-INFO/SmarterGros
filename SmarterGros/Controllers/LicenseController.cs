using Microsoft.AspNetCore.Mvc;
using SmarterGros.Services;

namespace SmarterGros.Controllers
{
    /// <summary>
    /// 🔐 Controller لإدارة الترخيص
    /// </summary>
    public class LicenseController : Controller
    {
        private readonly ILicenseService _licenseService;
        private readonly IHardwareIdService _hardwareIdService;

        public LicenseController(
            ILicenseService licenseService,
            IHardwareIdService hardwareIdService)
        {
            _licenseService = licenseService;
            _hardwareIdService = hardwareIdService;
        }

        // ═══════════════════════════════════════════════════
        // ⏰ صفحة انتهاء التجربة
        // ═══════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Expired()
        {
            ViewBag.HardwareId = _hardwareIdService.GetDisplayHardwareId();
            ViewBag.FullHardwareId = _hardwareIdService.GetHardwareId();
            ViewBag.License = await _licenseService.GetCurrentLicenseAsync();

            return View();
        }

        // ═══════════════════════════════════════════════════
        // 🔑 صفحة التفعيل
        // ═══════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Activate()
        {
            ViewBag.HardwareId = _hardwareIdService.GetDisplayHardwareId();
            ViewBag.FullHardwareId = _hardwareIdService.GetHardwareId();
            ViewBag.License = await _licenseService.GetCurrentLicenseAsync();
            ViewBag.RemainingDays = await _licenseService.GetRemainingDaysAsync();

            return View();
        }

        // ═══════════════════════════════════════════════════
        // 🚫 صفحة الترخيص غير صالح (تلاعب بالتاريخ)
        // ═══════════════════════════════════════════════════

        [HttpGet]
        public IActionResult Invalid()
        {
            ViewBag.HardwareId = _hardwareIdService.GetDisplayHardwareId();
            return View();
        }

        // ═══════════════════════════════════════════════════
        // ✅ معالجة التفعيل
        // ═══════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateKey(string activationKey)
        {
            if (string.IsNullOrWhiteSpace(activationKey))
            {
                return Json(new { success = false, message = "❌ يرجى إدخال مفتاح التفعيل" });
            }

            var (success, message) = await _licenseService.ActivateLicenseAsync(activationKey);

            return Json(new
            {
                success,
                message,
                redirectUrl = success ? Url.Action("Index", "Dashboard") : null
            });
        }

        // ═══════════════════════════════════════════════════
        // 📋 نسخ Hardware ID
        // ═══════════════════════════════════════════════════

        [HttpGet]
        public IActionResult GetHardwareInfo()
        {
            return Json(new
            {
                hardwareId = _hardwareIdService.GetDisplayHardwareId(),
                fullHardwareId = _hardwareIdService.GetHardwareId()
            });
        }


        // ═══════════════════════════════════════════════════
        // 📊 صفحة معلومات الترخيص
        // ═══════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Info()
        {
            ViewBag.HardwareId = _hardwareIdService.GetDisplayHardwareId();
            ViewBag.FullHardwareId = _hardwareIdService.GetHardwareId();
            ViewBag.License = await _licenseService.GetCurrentLicenseAsync();
            ViewBag.Status = await _licenseService.CheckLicenseAsync();
            ViewBag.RemainingDays = await _licenseService.GetRemainingDaysAsync();

            return View();
        }

    }
}