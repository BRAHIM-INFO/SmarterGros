using Microsoft.AspNetCore.Identity;
using SmarterGros.Models;

namespace SmarterGros.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roles = { "مدير النظام", "مدير المخزن", "مسؤول المبيعات", "مسؤول المشتريات", "مستخدم عادي" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            if (await userManager.FindByNameAsync("admin") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@smartergros.com",
                    FullName = "مدير النظام",
                    Role = "مدير النظام",
                    IsActive = true,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "مدير النظام");
            }

            if (!context.CompanySettings.Any())
            {
                context.CompanySettings.Add(new CompanySettings
                {
                    CompanyName = "SmarterGros",
                    CompanyType = "تجارة بالجملة",
                    Currency = "دج"
                });
                await context.SaveChangesAsync();
            }

            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "مواد غذائية" },
                    new Category { Name = "منظفات" },
                    new Category { Name = "أدوات" },
                    new Category { Name = "مشروبات" }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}