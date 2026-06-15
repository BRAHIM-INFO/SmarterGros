using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SmarterGros.Models;
using System.Security.Claims;

namespace SmarterGros.Security
{
    /// <summary>
    /// متطلب الصلاحية - يحدد ما هي الصلاحية المطلوبة
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }

    /// <summary>
    /// المعالج الذي يفحص فعلاً هل المستخدم لديه الصلاحية أم لا
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PermissionAuthorizationHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// الدالة الرئيسية: تتحقق هل المستخدم لديه الصلاحية المطلوبة
        /// </summary>
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // 1️⃣ التحقق إذا كان المستخدم مسجل دخول
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                return; // غير مسجل = لا صلاحية
            }

            // 2️⃣ الحصول على المستخدم من قاعدة البيانات
            var user = await _userManager.GetUserAsync(context.User);
            if (user == null)
            {
                return; // مستخدم غير موجود
            }

            // 3️⃣ التحقق من أن الحساب نشط
            if (!user.IsActive)
            {
                return; // حساب معطل
            }

            // 4️⃣ ⭐ مدير النظام له كل الصلاحيات تلقائياً
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains(RolePermissions.Roles.SystemAdmin))
            {
                context.Succeed(requirement); // ✅ سماح كامل
                return;
            }

            // 5️⃣ التحقق من صلاحيات الأدوار
            foreach (var roleName in userRoles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null) continue;

                // الحصول على Claims (الصلاحيات) لهذا الدور
                var roleClaims = await _roleManager.GetClaimsAsync(role);

                // التحقق إذا كانت الصلاحية المطلوبة موجودة
                var hasPermission = roleClaims.Any(c =>
                    c.Type == PermissionConstants.PermissionClaimType &&
                    c.Value == requirement.Permission);

                if (hasPermission)
                {
                    context.Succeed(requirement); // ✅ لديه الصلاحية
                    return;
                }
            }

            // 6️⃣ التحقق من الصلاحيات الفردية للمستخدم (إن وجدت)
            var userClaims = await _userManager.GetClaimsAsync(user);
            var hasUserPermission = userClaims.Any(c =>
                c.Type == PermissionConstants.PermissionClaimType &&
                c.Value == requirement.Permission);

            if (hasUserPermission)
            {
                context.Succeed(requirement); // ✅ لديه الصلاحية الفردية
                return;
            }

            // 7️⃣ إذا وصلنا هنا = لا توجد صلاحية
            // لا نفعل شيئاً، التفويض سيفشل تلقائياً
        }
    }
}