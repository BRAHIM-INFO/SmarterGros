using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace SmarterGros.Security
{
    /// <summary>
    /// مزود السياسات الديناميكي
    /// 
    /// المشكلة: ASP.NET افتراضياً يتطلب تسجيل كل Policy يدوياً في Program.cs
    /// مثل: options.AddPolicy("Permission:Products.View", ...)
    /// 
    /// الحل: هذا الكلاس يُنشئ Policies تلقائياً عند الطلب!
    /// </summary>
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        // المزود الافتراضي (للـ Policies العادية مثل [Authorize])
        public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            // إنشاء المزود الافتراضي للـ Policies العادية
            FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        /// <summary>
        /// السياسة الافتراضية (عند استخدام [Authorize] بدون تحديد Policy)
        /// </summary>
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
            => FallbackPolicyProvider.GetDefaultPolicyAsync();

        /// <summary>
        /// السياسة المُطبقة عند فشل التفويض
        /// </summary>
        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
            => FallbackPolicyProvider.GetFallbackPolicyAsync();

        /// <summary>
        /// ⭐ الدالة الرئيسية: تُنشئ Policy ديناميكياً بناءً على اسمها
        /// </summary>
        /// <param name="policyName">اسم Policy (مثل: "Permission:Products.View")</param>
        public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // 1️⃣ التحقق إذا كانت Policy خاصة بالصلاحيات
            if (policyName.StartsWith(PermissionConstants.PolicyPrefix,
                StringComparison.OrdinalIgnoreCase))
            {
                // 2️⃣ استخراج اسم الصلاحية الفعلي
                // مثال: "Permission:Products.View" → "Products.View"
                var permission = policyName.Substring(PermissionConstants.PolicyPrefix.Length);

                // 3️⃣ بناء Policy جديد ديناميكياً
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new PermissionRequirement(permission));

                // 4️⃣ إرجاع Policy الجاهز
                return policy.Build();
            }

            // 5️⃣ إذا لم تكن Policy خاصة بالصلاحيات
            //    استخدم المزود الافتراضي (لـ [Authorize] العادي)
            return await FallbackPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}