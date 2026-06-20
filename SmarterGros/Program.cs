using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmarterGros.Data;
using SmarterGros.Middleware;
using SmarterGros.Models;
using SmarterGros.Security;
using SmarterGros.Services; // ✅ جديد

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

// ✅ خدمة لوحة التحكم
builder.Services.AddScoped<IDashboardService, DashboardService>();

// ✅ خدمة التقارير
builder.Services.AddScoped<IReportsService, ReportsService>();

// ✅ خدمات الترخيص
builder.Services.AddScoped<IHardwareIdService, HardwareIdService>();
builder.Services.AddScoped<ILicenseService, LicenseService>();

// تسجيل مزود السياسات الديناميكي
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

// تسجيل المعالج (Handler) للتحقق من الصلاحيات
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// ✅ خدمة مرتجعات المشتريات - جديد!

builder.Services.AddScoped<IPurchaseReturnService, PurchaseReturnService>();

// ✅ خدمة الصندوق
builder.Services.AddScoped<ICashRegisterService, CashRegisterService>();

// ✅ خدمة المبيعات
builder.Services.AddScoped<ISaleService, SaleService>();
// ═══════════════════════════════════════════════════
// 🎮 MVC
// ═══════════════════════════════════════════════════
builder.Services.AddControllersWithViews();

// ═══════════════════════════════════════════════════
// 📝 نظام التتبع (Activity Log) - ✅ جديد
// ═══════════════════════════════════════════════════
builder.Services.AddHttpContextAccessor(); // للحصول على IP والمتصفح
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
// ✅ خدمة المشتريات
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
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
// 🔐 التحقق من الترخيص
app.UseLicenseCheck();




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

