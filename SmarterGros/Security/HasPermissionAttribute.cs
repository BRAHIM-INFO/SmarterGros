using Microsoft.AspNetCore.Authorization;

namespace SmarterGros.Security
{
    /// <summary>
    /// Attribute لحماية الـ Actions و Controllers بصلاحية معينة
    /// 
    /// الاستخدام:
    /// [HasPermission(Permissions.Products.View)]
    /// public IActionResult Index() { ... }
    /// </summary>
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// إنشاء Attribute مع صلاحية محددة
        /// </summary>
        /// <param name="permission">اسم الصلاحية (مثل: Permissions.Products.View)</param>
        public HasPermissionAttribute(string permission)
        {
            // نخزن الصلاحية كـ Policy ليتم التعامل معها لاحقاً
            Policy = $"{PermissionConstants.PolicyPrefix}{permission}";
        }
    }

    /// <summary>
    /// ثوابت نظام الصلاحيات
    /// </summary>
    public static class PermissionConstants
    {
        /// <summary>
        /// البادئة المستخدمة في Policy الصلاحيات
        /// تساعدنا على التمييز بين Policies الصلاحيات وغيرها
        /// </summary>
        public const string PolicyPrefix = "Permission:";

        /// <summary>
        /// نوع الـ Claim الذي يحمل الصلاحيات
        /// </summary>
        public const string PermissionClaimType = "Permission";
    }
}