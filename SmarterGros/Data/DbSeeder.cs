using Microsoft.AspNetCore.Identity;
using SmarterGros.Models;
using SmarterGros.Security; // ✅ جديد
using System.Security.Claims;

namespace SmarterGros.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // ═══════════════════════════════════════════════════
            // 1️⃣ إنشاء الأدوار + الصلاحيات
            // ═══════════════════════════════════════════════════
            await SeedRolesAndPermissionsAsync(roleManager);

            // ═══════════════════════════════════════════════════
            // 2️⃣ إنشاء مستخدم Admin افتراضي
            // ═══════════════════════════════════════════════════
            await SeedAdminUserAsync(userManager);

            // ═══════════════════════════════════════════════════
            // 3️⃣ بيانات الشركة الافتراضية
            // ═══════════════════════════════════════════════════
            await SeedCompanySettingsAsync(context);

            // ═══════════════════════════════════════════════════
            // 4️⃣ الفئات الافتراضية
            // ═══════════════════════════════════════════════════
            await SeedCategoriesAsync(context);
        }

        // ═══════════════════════════════════════════════════
        // 🔐 إنشاء الأدوار وتعيين الصلاحيات
        // ═══════════════════════════════════════════════════
        private static async Task SeedRolesAndPermissionsAsync(RoleManager<IdentityRole> roleManager)
        {
            // الحصول على قائمة الأدوار من RolePermissions
            var roles = RolePermissions.GetAllRoles();

            foreach (var roleName in roles)
            {
                // إنشاء الدور إذا لم يكن موجوداً
                var role = await roleManager.FindByNameAsync(roleName);
                if (role == null)
                {
                    role = new IdentityRole(roleName);
                    await roleManager.CreateAsync(role);
                    Console.WriteLine($"✅ تم إنشاء دور: {roleName}");
                }

                // الحصول على الصلاحيات الافتراضية لهذا الدور
                var defaultPermissions = RolePermissions.GetDefaultPermissionsForRole(roleName);

                // الحصول على Claims الحالية للدور
                var existingClaims = await roleManager.GetClaimsAsync(role);
                var existingPermissions = existingClaims
                    .Where(c => c.Type == PermissionConstants.PermissionClaimType)
                    .Select(c => c.Value)
                    .ToList();

                // إضافة الصلاحيات الجديدة (التي لم تُضف من قبل)
                foreach (var permission in defaultPermissions)
                {
                    if (!existingPermissions.Contains(permission))
                    {
                        await roleManager.AddClaimAsync(
                            role,
                            new Claim(PermissionConstants.PermissionClaimType, permission)
                        );
                    }
                }

                Console.WriteLine($"   ↳ {defaultPermissions.Count} صلاحية معينة لدور: {roleName}");
            }
        }

        // ═══════════════════════════════════════════════════
        // 👤 إنشاء مستخدم Admin
        // ═══════════════════════════════════════════════════
        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            if (await userManager.FindByNameAsync("admin") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@smartergros.com",
                    FullName = "مدير النظام",
                    Role = RolePermissions.Roles.SystemAdmin,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now
                };

                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, RolePermissions.Roles.SystemAdmin);
                    Console.WriteLine("✅ تم إنشاء مستخدم admin افتراضي (Admin@123)");
                }
                else
                {
                    Console.WriteLine($"❌ فشل إنشاء admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        // ═══════════════════════════════════════════════════
        // 🏢 إعدادات الشركة
        // ═══════════════════════════════════════════════════
        private static async Task SeedCompanySettingsAsync(ApplicationDbContext context)
        {
            if (!context.CompanySettings.Any())
            {
                context.CompanySettings.Add(new CompanySettings
                {
                    CompanyName = "SmarterGros",
                    CompanyType = "تجارة بالجملة",
                    Currency = "دج"
                });
                await context.SaveChangesAsync();
                Console.WriteLine("✅ تم إنشاء إعدادات الشركة الافتراضية");
            }
        }

        // ═══════════════════════════════════════════════════
        // 🏷️ الفئات الافتراضية
        // ═══════════════════════════════════════════════════
        private static async Task SeedCategoriesAsync(ApplicationDbContext context)
        {
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "مواد غذائية" },
                    new Category { Name = "منظفات" },
                    new Category { Name = "أدوات" },
                    new Category { Name = "مشروبات" }
                );
                await context.SaveChangesAsync();
                Console.WriteLine("✅ تم إنشاء الفئات الافتراضية");
            }
        }
    }
}




//using Microsoft.AspNetCore.Identity;
//using SmarterGros.Models;

//namespace SmarterGros.Data
//{
//    public static class DbSeeder
//    {
//        public static async Task SeedAsync(IServiceProvider serviceProvider)
//        {
//            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
//            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

//            string[] roles = { "مدير النظام", "مدير المخزن", "مسؤول المبيعات", "مسؤول المشتريات", "مستخدم عادي" };
//            foreach (var role in roles)
//            {
//                if (!await roleManager.RoleExistsAsync(role))
//                    await roleManager.CreateAsync(new IdentityRole(role));
//            }

//            if (await userManager.FindByNameAsync("admin") == null)
//            {
//                var admin = new ApplicationUser
//                {
//                    UserName = "admin",
//                    Email = "admin@smartergros.com",
//                    FullName = "مدير النظام",
//                    Role = "مدير النظام",
//                    IsActive = true,
//                    EmailConfirmed = true
//                };
//                var result = await userManager.CreateAsync(admin, "Admin@123");
//                if (result.Succeeded)
//                    await userManager.AddToRoleAsync(admin, "مدير النظام");
//            }

//            if (!context.CompanySettings.Any())
//            {
//                context.CompanySettings.Add(new CompanySettings
//                {
//                    CompanyName = "SmarterGros",
//                    CompanyType = "تجارة بالجملة",
//                    Currency = "دج"
//                });
//                await context.SaveChangesAsync();
//            }

//            if (!context.Categories.Any())
//            {
//                context.Categories.AddRange(
//                    new Category { Name = "مواد غذائية" },
//                    new Category { Name = "منظفات" },
//                    new Category { Name = "أدوات" },
//                    new Category { Name = "مشروبات" }
//                );
//                await context.SaveChangesAsync();
//            }
//        }
//    }
//}