using Microsoft.AspNetCore.Mvc;
using SmarterGros.Services;

namespace SmarterGros.Controllers
{
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

        [HttpGet]
        public async Task<IActionResult> Expired()
        {
            ViewBag.HardwareId = _hardwareIdService.GetHardwareId();  // ✅ الكامل
            ViewBag.FullHardwareId = _hardwareIdService.GetHardwareId();
            ViewBag.License = await _licenseService.GetCurrentLicenseAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Activate()
        {
            ViewBag.HardwareId = _hardwareIdService.GetHardwareId();  // ✅ الكامل
            ViewBag.FullHardwareId = _hardwareIdService.GetHardwareId();
            ViewBag.License = await _licenseService.GetCurrentLicenseAsync();
            ViewBag.RemainingDays = await _licenseService.GetRemainingDaysAsync();
            return View();
        }

        [HttpGet]
        public IActionResult Invalid()
        {
            ViewBag.HardwareId = _hardwareIdService.GetHardwareId();  // ✅ الكامل
            ViewBag.FullHardwareId = _hardwareIdService.GetHardwareId();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateKey(string activationKey)
        {
            if (string.IsNullOrWhiteSpace(activationKey))
                return Json(new { success = false, message = "❌ يرجى إدخال مفتاح التفعيل" });

            var (success, message) = await _licenseService.ActivateLicenseAsync(activationKey);

            return Json(new
            {
                success,
                message,
                redirectUrl = success ? Url.Action("Index", "Dashboard") : null
            });
        }

        [HttpGet]
        public IActionResult GetHardwareInfo()
        {
            return Json(new
            {
                hardwareId = _hardwareIdService.GetHardwareId(),  // ✅ الكامل
                fullHardwareId = _hardwareIdService.GetHardwareId()
            });
        }

        [HttpGet]
        public async Task<IActionResult> Info()
        {
            ViewBag.HardwareId = _hardwareIdService.GetHardwareId();  // ✅ الكامل
            ViewBag.FullHardwareId = _hardwareIdService.GetHardwareId();
            ViewBag.License = await _licenseService.GetCurrentLicenseAsync();
            ViewBag.Status = await _licenseService.CheckLicenseAsync();
            ViewBag.RemainingDays = await _licenseService.GetRemainingDaysAsync();
            return View();
        }
    }
}