using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmarterGros.Data;
using SmarterGros.Models;
using SmarterGros.Security; // ✅ جديد
using SmarterGros.ViewModels;

namespace SmarterGros.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public SettingsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // ═══════════════════════════════════════════════════
        // 🏢 إعدادات المؤسسة
        // ═══════════════════════════════════════════════════
        [HasPermission(Permissions.CompanySettings.View)]
        public async Task<IActionResult> Index()
        {
            var settings = await _context.CompanySettings.FirstOrDefaultAsync()
                           ?? new CompanySettings();
            return View(settings);
        }

        [HttpPost]
        [HasPermission(Permissions.CompanySettings.Edit)]
        public async Task<IActionResult> SaveCompany(CompanySettings model, IFormFile? logoFile)
        {
            var settings = await _context.CompanySettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = model;
                _context.CompanySettings.Add(settings);
            }
            else
            {
                settings.CompanyName = model.CompanyName;
                settings.CompanyType = model.CompanyType;
                settings.RC = model.RC;
                settings.NIF = model.NIF;
                settings.FoundingDate = model.FoundingDate;
                settings.Address = model.Address;
                settings.City = model.City;
                settings.PostalCode = model.PostalCode;
                settings.Phone = model.Phone;
                settings.Email = model.Email;
                settings.Website = model.Website;
                settings.Currency = model.Currency;
            }

            if (logoFile != null)
            {
                var dir = Path.Combine(_env.WebRootPath, "uploads", "logo");
                Directory.CreateDirectory(dir);
                var fileName = $"logo{Path.GetExtension(logoFile.FileName)}";
                var path = Path.Combine(dir, fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await logoFile.CopyToAsync(stream);
                settings.LogoPath = $"/uploads/logo/{fileName}";
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حفظ بيانات المؤسسة بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ═══════════════════════════════════════════════════
        // 👥 إدارة المستخدمين
        // ═══════════════════════════════════════════════════
        [HasPermission(Permissions.Users.View)]
        public async Task<IActionResult> Users(string? search, string? role, string? status)
        {
            var users = await _userManager.Users.ToListAsync();

            if (!string.IsNullOrEmpty(search))
                users = users.Where(u =>
                    u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.UserName!.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (u.Email != null && u.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
                ).ToList();

            if (!string.IsNullOrEmpty(role))
                users = users.Where(u => u.Role == role).ToList();

            if (!string.IsNullOrEmpty(status))
            {
                bool isActive = status == "active";
                users = users.Where(u => u.IsActive == isActive).ToList();
            }

            var userVMs = users.Select(u => new UserViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                UserName = u.UserName ?? "",
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                PhoneNumber = u.PhoneNumber
            }).OrderByDescending(u => u.CreatedAt).ToList();

            ViewBag.Search = search;
            ViewBag.RoleFilter = role;
            ViewBag.StatusFilter = status;
            ViewBag.TotalUsers = userVMs.Count;
            ViewBag.ActiveUsers = userVMs.Count(u => u.IsActive);
            ViewBag.Roles = GetRoles();

            return View(userVMs);
        }

        [HttpGet]
        [HasPermission(Permissions.Users.View)]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                id = user.Id,
                fullName = user.FullName,
                userName = user.UserName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                role = user.Role,
                isActive = user.IsActive
            });
        }

        [HttpPost]
        [HasPermission(Permissions.Users.Create)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserViewModel model)
        {
            if (string.IsNullOrEmpty(model.FullName))
                return Json(new { success = false, message = "الاسم الكامل مطلوب" });

            if (string.IsNullOrEmpty(model.UserName))
                return Json(new { success = false, message = "اسم المستخدم مطلوب" });

            if (await _userManager.FindByNameAsync(model.UserName) != null)
                return Json(new { success = false, message = "اسم المستخدم مستخدم مسبقاً" });

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Role = model.Role,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return Json(new { success = false, message = string.Join("، ", errors) });
            }

            await _userManager.AddToRoleAsync(user, model.Role);

            return Json(new
            {
                success = true,
                message = $"تم إنشاء المستخدم {model.FullName} بنجاح",
                userId = user.Id
            });
        }

        [HttpPost]
        [HasPermission(Permissions.Users.Edit)]
        public async Task<IActionResult> EditUser([FromBody] EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return Json(new { success = false, message = "المستخدم غير موجود" });

            var existingUser = await _userManager.FindByNameAsync(model.UserName);
            if (existingUser != null && existingUser.Id != model.Id)
                return Json(new { success = false, message = "اسم المستخدم مستخدم مسبقاً" });

            var oldRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, oldRoles);
            await _userManager.AddToRoleAsync(user, model.Role);

            user.FullName = model.FullName;
            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Role = model.Role;
            user.IsActive = model.IsActive;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return Json(new { success = false, message = "فشل تحديث البيانات" });

            return Json(new { success = true, message = "تم تعديل بيانات المستخدم بنجاح" });
        }

        [HttpPost]
        [HasPermission(Permissions.Users.ChangePassword)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return Json(new { success = false, message = "المستخدم غير موجود" });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return Json(new { success = false, message = string.Join("، ", errors) });
            }

            return Json(new { success = true, message = "تم تغيير كلمة المرور بنجاح" });
        }

        [HttpPost]
        [HasPermission(Permissions.Users.ToggleActive)]
        public async Task<IActionResult> ToggleUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Json(new { success = false, message = "المستخدم غير موجود" });

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
                return Json(new { success = false, message = "لا يمكنك تعطيل حسابك الخاص" });

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            return Json(new
            {
                success = true,
                isActive = user.IsActive,
                message = user.IsActive ? "تم تفعيل المستخدم" : "تم تعطيل المستخدم"
            });
        }

        [HttpPost]
        [HasPermission(Permissions.Users.Delete)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Json(new { success = false, message = "المستخدم غير موجود" });

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
                return Json(new { success = false, message = "لا يمكنك حذف حسابك الخاص" });

            await _userManager.DeleteAsync(user);
            return Json(new { success = true, message = "تم حذف المستخدم بنجاح" });
        }

        // ═══════════════════════════════════════════════════
        // 💾 قاعدة البيانات
        // ═══════════════════════════════════════════════════
        [HasPermission(Permissions.Database.View)]
        public async Task<IActionResult> Database()
        {
            var connectionString = _context.Database.GetConnectionString() ?? "";
            var dbName = _context.Database.GetDbConnection().Database;
            var serverName = _context.Database.GetDbConnection().DataSource;

            long dbSize = 0;
            try
            {
                var sizeSql = $"SELECT SUM(size * 8) FROM sys.database_files";
                var conn = _context.Database.GetDbConnection();
                await conn.OpenAsync();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sizeSql;
                var result = await cmd.ExecuteScalarAsync();
                dbSize = result != null ? Convert.ToInt64(result) : 0;
                await conn.CloseAsync();
            }
            catch { dbSize = 0; }

            var backupDir = Path.Combine(_env.WebRootPath, "backups");
            string? lastBackupFile = null;
            DateTime? lastBackupDate = null;

            if (Directory.Exists(backupDir))
            {
                var files = Directory.GetFiles(backupDir, "*.bak")
                                     .OrderByDescending(f => System.IO.File.GetCreationTime(f))
                                     .ToArray();
                if (files.Any())
                {
                    lastBackupFile = Path.GetFileName(files[0]);
                    lastBackupDate = System.IO.File.GetCreationTime(files[0]);
                }
            }

            var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
            var totalSuppliers = await _context.Suppliers.CountAsync(s => s.IsActive);
            var totalCustomers = await _context.Customers.CountAsync(c => c.IsActive);
            var totalSales = await _context.Sales.CountAsync();
            var totalPurchases = await _context.Purchases.CountAsync();
            var totalMovements = await _context.StockMovements.CountAsync();

            ViewBag.DbName = dbName;
            ViewBag.ServerName = serverName;
            ViewBag.DbSizeKB = dbSize;
            ViewBag.LastBackupFile = lastBackupFile;
            ViewBag.LastBackupDate = lastBackupDate;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalSuppliers = totalSuppliers;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.TotalSales = totalSales;
            ViewBag.TotalPurchases = totalPurchases;
            ViewBag.TotalMovements = totalMovements;
            ViewBag.BackupDir = backupDir;

            return View();
        }

        [HttpPost]
        [HasPermission(Permissions.Database.Backup)]
        public async Task<IActionResult> CreateBackup()
        {
            try
            {
                var dbName = _context.Database.GetDbConnection().Database;

                var backupDirSql = @"
            DECLARE @path NVARCHAR(500);
            EXEC master.dbo.xp_instance_regread
                N'HKEY_LOCAL_MACHINE',
                N'Software\Microsoft\MSSQLServer\MSSQLServer',
                N'BackupDirectory',
                @path OUTPUT;
            SELECT @path;";

                string sqlBackupDir = "C:\\Backups";
                try
                {
                    var conn = _context.Database.GetDbConnection();
                    await conn.OpenAsync();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = backupDirSql;
                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null && !string.IsNullOrEmpty(result.ToString()))
                        sqlBackupDir = result.ToString()!;
                    await conn.CloseAsync();
                }
                catch { }

                var projectBackupDir = Path.Combine(sqlBackupDir, "SmarterGros");

                try { Directory.CreateDirectory(projectBackupDir); }
                catch { projectBackupDir = sqlBackupDir; }

                var fileName = $"SmarterGros_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                var backupPath = Path.Combine(projectBackupDir, fileName);

                var sql = $@"BACKUP DATABASE [{dbName}]
                     TO DISK = N'{backupPath}'
                     WITH FORMAT, INIT,
                     NAME = N'SmarterGros-Full Backup',
                     SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                await _context.Database.ExecuteSqlRawAsync(sql);

                var webBackupDir = Path.Combine(_env.WebRootPath, "backups");
                Directory.CreateDirectory(webBackupDir);
                var webBackupPath = Path.Combine(webBackupDir, fileName);

                try { System.IO.File.Copy(backupPath, webBackupPath, true); }
                catch { }

                return Json(new
                {
                    success = true,
                    message = "تم إنشاء النسخة الاحتياطية بنجاح",
                    fileName = fileName,
                    date = DateTime.Now.ToString("yyyy/MM/dd - hh:mm tt"),
                    backupPath = backupPath
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"فشل: {ex.Message}"
                });
            }
        }

        [HttpPost]
        [HasPermission(Permissions.Database.Restore)]
        public async Task<IActionResult> RestoreBackup(IFormFile backupFile)
        {
            if (backupFile == null || backupFile.Length == 0)
                return Json(new { success = false, message = "يرجى اختيار ملف النسخة الاحتياطية" });

            try
            {
                var backupDir = Path.Combine(_env.WebRootPath, "backups");
                Directory.CreateDirectory(backupDir);

                var filePath = Path.Combine(backupDir, backupFile.FileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await backupFile.CopyToAsync(stream);
                stream.Close();

                var dbName = _context.Database.GetDbConnection().Database;

                var sqlSetSingle = $@"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                var sqlRestore = $@"RESTORE DATABASE [{dbName}]
                              FROM DISK = N'{filePath}'
                              WITH REPLACE, RECOVERY";
                var sqlSetMulti = $@"ALTER DATABASE [{dbName}] SET MULTI_USER";

                await _context.Database.ExecuteSqlRawAsync(sqlSetSingle);
                await _context.Database.ExecuteSqlRawAsync(sqlRestore);
                await _context.Database.ExecuteSqlRawAsync(sqlSetMulti);

                return Json(new { success = true, message = "تم استعادة قاعدة البيانات بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"فشل الاستعادة: {ex.Message}" });
            }
        }

        [HasPermission(Permissions.Database.Backup)]
        public IActionResult DownloadBackup(string fileName)
        {
            var backupDir = Path.Combine(_env.WebRootPath, "backups");
            var filePath = Path.Combine(backupDir, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }

        [HttpGet]
        [HasPermission(Permissions.Database.View)]
        public IActionResult GetBackups()
        {
            var backupDir = Path.Combine(_env.WebRootPath, "backups");
            if (!Directory.Exists(backupDir))
                return Json(new { backups = new List<object>() });

            var files = Directory.GetFiles(backupDir, "*.bak")
                .OrderByDescending(f => System.IO.File.GetCreationTime(f))
                .Select(f => new
                {
                    fileName = Path.GetFileName(f),
                    date = System.IO.File.GetCreationTime(f).ToString("yyyy/MM/dd - hh:mm tt"),
                    sizeMB = (new FileInfo(f).Length / 1024.0 / 1024.0).ToString("F2")
                })
                .ToList();

            return Json(new { backups = files });
        }

        [HttpPost]
        [HasPermission(Permissions.Database.Clear)]
        public async Task<IActionResult> ClearDatabase()
        {
            try
            {
                _context.StockMovements.RemoveRange(_context.StockMovements);
                _context.SaleItems.RemoveRange(_context.SaleItems);
                _context.PurchaseItems.RemoveRange(_context.PurchaseItems);
                _context.Sales.RemoveRange(_context.Sales);
                _context.Purchases.RemoveRange(_context.Purchases);
                _context.Products.RemoveRange(_context.Products);
                _context.Categories.RemoveRange(_context.Categories);
                _context.Suppliers.RemoveRange(_context.Suppliers);
                _context.Customers.RemoveRange(_context.Customers);

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "تم مسح جميع البيانات بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"فشل المسح: {ex.Message}" });
            }
        }
         
        // ═══════════════════════════════════════════════════
        // 🎧 الدعم
        // ═══════════════════════════════════════════════════
        [HasPermission(Permissions.Support.View)]
        public IActionResult Support() => View();

        // ═══════════════════════════════════════════════════
        // 🛠️ مساعدات
        // ═══════════════════════════════════════════════════
        private List<string> GetRoles() => RolePermissions.GetAllRoles();

        // ═══════════════════════════════════════════════════
        // 🛡️ إدارة الصلاحيات
        // ═══════════════════════════════════════════════════
        [HasPermission(Permissions.Roles.View)]
        public async Task<IActionResult> RolesPermissions(string? roleName = null)
        {
            // الحصول على جميع الأدوار
            var allRoles = RolePermissions.GetAllRoles();

            // إذا لم يتم تحديد دور، نأخذ الأول (مدير النظام)
            if (string.IsNullOrEmpty(roleName))
                roleName = allRoles.First();

            var roleManager = HttpContext.RequestServices
                .GetRequiredService<RoleManager<IdentityRole>>();

            // الحصول على الدور
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                TempData["Error"] = "الدور غير موجود";
                return RedirectToAction(nameof(RolesPermissions));
            }

            // الحصول على صلاحيات الدور الحالية
            var roleClaims = await roleManager.GetClaimsAsync(role);
            var currentPermissions = roleClaims
                .Where(c => c.Type == PermissionConstants.PermissionClaimType)
                .Select(c => c.Value)
                .ToHashSet();

            // الحصول على جميع الصلاحيات المتاحة
            var allPermissions = PermissionsList.GetAllPermissions();

            // إعداد البيانات للـ View
            ViewBag.AllRoles = allRoles;
            ViewBag.SelectedRole = roleName;
            ViewBag.AllPermissions = allPermissions;
            ViewBag.CurrentPermissions = currentPermissions;
            ViewBag.IsSystemAdmin = RolePermissions.IsSystemAdmin(roleName);

            // إحصائيات
            ViewBag.TotalPermissions = PermissionsList.GetAllPermissionKeys().Count;
            ViewBag.AssignedPermissions = currentPermissions.Count;

            return View();
        }

        // POST: حفظ الصلاحيات
        [HttpPost]
        [HasPermission(Permissions.Roles.ManagePermissions)]
        public async Task<IActionResult> SaveRolePermissions([FromBody] SaveRolePermissionsRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.RoleName))
                return Json(new { success = false, message = "بيانات غير صالحة" });

            // منع تعديل صلاحيات مدير النظام
            if (RolePermissions.IsSystemAdmin(request.RoleName))
                return Json(new { success = false, message = "لا يمكن تعديل صلاحيات مدير النظام" });

            var roleManager = HttpContext.RequestServices
                .GetRequiredService<RoleManager<IdentityRole>>();

            var role = await roleManager.FindByNameAsync(request.RoleName);
            if (role == null)
                return Json(new { success = false, message = "الدور غير موجود" });

            // الحصول على الصلاحيات الحالية
            var currentClaims = await roleManager.GetClaimsAsync(role);
            var currentPermissions = currentClaims
                .Where(c => c.Type == PermissionConstants.PermissionClaimType)
                .ToList();

            // حذف جميع الصلاحيات الحالية
            foreach (var claim in currentPermissions)
            {
                await roleManager.RemoveClaimAsync(role, claim);
            }

            // إضافة الصلاحيات الجديدة
            if (request.Permissions != null && request.Permissions.Any())
            {
                foreach (var permission in request.Permissions)
                {
                    await roleManager.AddClaimAsync(
                        role,
                        new System.Security.Claims.Claim(
                            PermissionConstants.PermissionClaimType,
                            permission
                        )
                    );
                }
            }

            return Json(new
            {
                success = true,
                message = $"تم حفظ صلاحيات دور '{request.RoleName}' بنجاح",
                count = request.Permissions?.Count ?? 0
            });
        }

        // POST: إعادة تعيين الصلاحيات الافتراضية
        [HttpPost]
        [HasPermission(Permissions.Roles.ManagePermissions)]
        public async Task<IActionResult> ResetRolePermissions(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return Json(new { success = false, message = "اسم الدور مطلوب" });

            if (RolePermissions.IsSystemAdmin(roleName))
                return Json(new { success = false, message = "لا يمكن تعديل صلاحيات مدير النظام" });

            var roleManager = HttpContext.RequestServices
                .GetRequiredService<RoleManager<IdentityRole>>();

            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
                return Json(new { success = false, message = "الدور غير موجود" });

            // حذف الصلاحيات الحالية
            var currentClaims = await roleManager.GetClaimsAsync(role);
            var permissionClaims = currentClaims
                .Where(c => c.Type == PermissionConstants.PermissionClaimType)
                .ToList();

            foreach (var claim in permissionClaims)
            {
                await roleManager.RemoveClaimAsync(role, claim);
            }

            // إضافة الصلاحيات الافتراضية
            var defaultPermissions = RolePermissions.GetDefaultPermissionsForRole(roleName);
            foreach (var permission in defaultPermissions)
            {
                await roleManager.AddClaimAsync(
                    role,
                    new System.Security.Claims.Claim(
                        PermissionConstants.PermissionClaimType,
                        permission
                    )
                );
            }

            return Json(new
            {
                success = true,
                message = $"تم إعادة تعيين الصلاحيات الافتراضية لدور '{roleName}'",
                count = defaultPermissions.Count
            });
        }

        // ═══════════════════════════════════════════════════
        // 📦 Request Models
        // ═══════════════════════════════════════════════════
        public class SaveRolePermissionsRequest
        {
            public string RoleName { get; set; } = string.Empty;
            public List<string> Permissions { get; set; } = new();
        }

    }
}
