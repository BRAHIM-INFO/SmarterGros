using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmarterGros.Data;
using SmarterGros.Models;
using SmarterGros.Security; // ✅ جديد

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════
// 📦 قاعدة البيانات
// ═══════════════════════════════════════════════════
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ═══════════════════════════════════════════════════
// 🔐 Identity (المستخدمين والأدوار)
// ═══════════════════════════════════════════════════
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ═══════════════════════════════════════════════════
// 🍪 إعدادات الـ Cookies
// ═══════════════════════════════════════════════════
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied"; // ✅ صفحة رفض الوصول
});

// ═══════════════════════════════════════════════════
// 🛡️ نظام الصلاحيات (Permissions System) - ✅ جديد
// ═══════════════════════════════════════════════════

// تسجيل مزود السياسات الديناميكي
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

// تسجيل المعالج (Handler) للتحقق من الصلاحيات
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// ═══════════════════════════════════════════════════
// 🎮 MVC
// ═══════════════════════════════════════════════════
builder.Services.AddControllersWithViews();

// ═══════════════════════════════════════════════════
// 🏗️ بناء التطبيق
// ═══════════════════════════════════════════════════
var app = builder.Build();

// ═══════════════════════════════════════════════════
// ⚙️ Pipeline (الترتيب مهم جداً!)
// ═══════════════════════════════════════════════════
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); // 1️⃣ التحقق من تسجيل الدخول
app.UseAuthorization();  // 2️⃣ التحقق من الصلاحيات

// ═══════════════════════════════════════════════════
// 🗺️ Routes
// ═══════════════════════════════════════════════════
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// ═══════════════════════════════════════════════════
// 🌱 Seeding (البيانات الافتراضية)
// ═══════════════════════════════════════════════════
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        await DbSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding database: {ex.Message}");
    }
}

app.Run();


//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using SmarterGros.Data;
//using SmarterGros.Models;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
//{
//    options.Password.RequireDigit = true;
//    options.Password.RequiredLength = 6;
//    options.Password.RequireNonAlphanumeric = false;
//    options.Password.RequireUppercase = false;
//})
//.AddEntityFrameworkStores<ApplicationDbContext>()
//.AddDefaultTokenProviders();

//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.LoginPath = "/Account/Login";
//    options.LogoutPath = "/Account/Logout";
//    options.AccessDeniedPath = "/Account/AccessDenied";
//});

//builder.Services.AddControllersWithViews();

//var app = builder.Build();

//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();
//app.UseRouting();
//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Account}/{action=Login}/{id?}");

//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    try
//    {
//        var context = services.GetRequiredService<ApplicationDbContext>();
//        context.Database.Migrate();
//        await DbSeeder.SeedAsync(services);
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Error seeding database: {ex.Message}");
//    }
//}

//app.Run(); 