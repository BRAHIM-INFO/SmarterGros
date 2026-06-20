using SmarterGros.Services;

namespace SmarterGros.Middleware
{
    /// <summary>
    /// 🔐 Middleware للتحقق من الترخيص في كل طلب
    /// </summary>
    public class LicenseMiddleware
    {
        private readonly RequestDelegate _next;

        // المسارات المسموح بها بدون ترخيص
        private readonly string[] _allowedPaths = new[]
        {
            "/License",              // صفحات الترخيص
            "/Account/Login",        // تسجيل الدخول
            "/Account/Logout",       // تسجيل الخروج
            "/lib",                  // مكتبات
            "/css",                  // ملفات CSS
            "/js",                   // ملفات JS
            "/images",               // الصور
            "/favicon.ico"           // الأيقونة
        };

        public LicenseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILicenseService licenseService)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // تجاهل المسارات المسموح بها
            if (_allowedPaths.Any(p => path.StartsWith(p.ToLower())))
            {
                await _next(context);
                return;
            }

            try
            {
                // التحقق من حالة الترخيص
                var status = await licenseService.CheckLicenseAsync();

                switch (status)
                {
                    case LicenseStatus.Expired:
                        // ❌ التجربة منتهية
                        context.Response.Redirect("/License/Expired");
                        return;

                    case LicenseStatus.Invalid:
                        // 🚫 محاولة تلاعب
                        context.Response.Redirect("/License/Invalid");
                        return;

                    case LicenseStatus.NotActivated:
                        // 🔓 لم يتم التفعيل
                        context.Response.Redirect("/License/Activate");
                        return;

                    case LicenseStatus.TrialActive:
                    case LicenseStatus.TrialExpiring:
                    case LicenseStatus.Active:
                        // ✅ يعمل بشكل طبيعي
                        await _next(context);
                        break;
                }
            }
            catch
            {
                // في حالة خطأ، نسمح بالمرور
                await _next(context);
            }
        }
    }

    /// <summary>
    /// Extension method لتسجيل Middleware
    /// </summary>
    public static class LicenseMiddlewareExtensions
    {
        public static IApplicationBuilder UseLicenseCheck(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LicenseMiddleware>();
        }
    }
}