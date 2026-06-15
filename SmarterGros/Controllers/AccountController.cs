using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmarterGros.Models;
using SmarterGros.Services; // ✅ جديد
using SmarterGros.ViewModels;

namespace SmarterGros.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IActivityLogService _activityLog; // ✅ جديد


        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
             IActivityLogService activityLog)// ✅ جديد  
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _activityLog = activityLog; // ✅ جديد
        }

        // ═══════════════════════════════════════════════════
        // 🔐 تسجيل الدخول
        // ═══════════════════════════════════════════════════
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null || !user.IsActive)
            {
                // ✅ تسجيل محاولة فاشلة
                await _activityLog.LogLoginAsync(
                    userName: model.UserName,
                    isSuccess: false,
                    errorMessage: user == null ? "مستخدم غير موجود" : "حساب معطل"
                );

                ModelState.AddModelError("", "اسم المستخدم أو كلمة المرور غير صحيحة");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.UserName, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                // ✅ تسجيل دخول ناجح
                await _activityLog.LogLoginAsync(
                    userName: model.UserName,
                    isSuccess: true
                );

                return RedirectToAction("Index", "Dashboard");
            }

            // ✅ تسجيل محاولة فاشلة (كلمة مرور خاطئة)
            await _activityLog.LogLoginAsync(
                userName: model.UserName,
                isSuccess: false,
                errorMessage: "كلمة مرور خاطئة"
            );

            ModelState.AddModelError("", "اسم المستخدم أو كلمة المرور غير صحيحة");
            return View(model);
        }

        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<IActionResult> Login(LoginViewModel model)
        //{
        //    if (!ModelState.IsValid) return View(model);

        //    var user = await _userManager.FindByNameAsync(model.UserName);
        //    if (user == null || !user.IsActive)
        //    {
        //        ModelState.AddModelError("", "اسم المستخدم أو كلمة المرور غير صحيحة");
        //        return View(model);
        //    }

        //    var result = await _signInManager.PasswordSignInAsync(
        //        model.UserName,
        //        model.Password,
        //        model.RememberMe,
        //        false);

        //    if (result.Succeeded)
        //        return RedirectToAction("Index", "Dashboard");

        //    ModelState.AddModelError("", "اسم المستخدم أو كلمة المرور غير صحيحة");
        //    return View(model);
        //}

        // ═══════════════════════════════════════════════════
        // 🚪 تسجيل الخروج
        // ═══════════════════════════════════════════════════
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // ✅ تسجيل الخروج قبل تنفيذه (لنحفظ بيانات المستخدم)
            await _activityLog.LogLogoutAsync();

            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // ═══════════════════════════════════════════════════
        // 🚫 صفحة رفض الوصول
        // ═══════════════════════════════════════════════════
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}


//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using SmarterGros.Models;
//using SmarterGros.ViewModels;

//namespace SmarterGros.Controllers
//{
//    public class AccountController : Controller
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly SignInManager<ApplicationUser> _signInManager;

//        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
//        {
//            _userManager = userManager;
//            _signInManager = signInManager;
//        }

//        [AllowAnonymous]
//        public IActionResult Login()
//        {
//            if (User.Identity?.IsAuthenticated == true)
//                return RedirectToAction("Index", "Dashboard");
//            return View();
//        }

//        [HttpPost]
//        [AllowAnonymous]
//        public async Task<IActionResult> Login(LoginViewModel model)
//        {
//            if (!ModelState.IsValid) return View(model);

//            var user = await _userManager.FindByNameAsync(model.UserName);
//            if (user == null || !user.IsActive)
//            {
//                ModelState.AddModelError("", "اسم المستخدم أو كلمة المرور غير صحيحة");
//                return View(model);
//            }

//            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);
//            if (result.Succeeded)
//                return RedirectToAction("Index", "Dashboard");

//            ModelState.AddModelError("", "اسم المستخدم أو كلمة المرور غير صحيحة");
//            return View(model);
//        }

//        [HttpPost]
//        [Authorize]
//        public async Task<IActionResult> Logout()
//        {
//            await _signInManager.SignOutAsync();
//            return RedirectToAction("Login");
//        }
//    }
//}