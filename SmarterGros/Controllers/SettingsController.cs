using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmarterGros.Data;
using SmarterGros.Models;
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

        // ===== COMPANY =====
        public async Task<IActionResult> Index()
        {
            var settings = await _context.CompanySettings.FirstOrDefaultAsync()
                           ?? new CompanySettings();
            return View(settings);
        }

        [HttpPost]
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

        // ===== USERS =====
        public async Task<IActionResult> Users(string? search, string? role, string? status)
        {
            var users = await _userManager.Users.ToListAsync();

            // Filter
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

        // GET: GetUser
        [HttpGet]
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

        // POST: CreateUser
        [HttpPost]
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

        // POST: EditUser
        [HttpPost]
        public async Task<IActionResult> EditUser([FromBody] EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return Json(new { success = false, message = "المستخدم غير موجود" });

            // Check username uniqueness
            var existingUser = await _userManager.FindByNameAsync(model.UserName);
            if (existingUser != null && existingUser.Id != model.Id)
                return Json(new { success = false, message = "اسم المستخدم مستخدم مسبقاً" });

            // Update role
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

        // POST: ChangePassword
        [HttpPost]
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

        // POST: ToggleUser
        [HttpPost]
        public async Task<IActionResult> ToggleUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Json(new { success = false, message = "المستخدم غير موجود" });

            // Prevent deactivating own account
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

        // POST: DeleteUser
        [HttpPost]
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

        // ===== DATABASE ===== 
        public async Task<IActionResult> Database()
        {
            var connectionString = _context.Database.GetConnectionString() ?? "";
            var dbName = _context.Database.GetDbConnection().Database;
            var serverName = _context.Database.GetDbConnection().DataSource;

            // Get database size
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

            // Get last backup info
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

            // Stats
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

        // POST: Backup
        [HttpPost]
        public async Task<IActionResult> CreateBackup()
        {
            try
            {
                var dbName = _context.Database.GetDbConnection().Database;

                // الحصول على مجلد النسخ الاحتياطية الافتراضي لـ SQL Server
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
                catch { /* استخدم المسار الافتراضي */ }

                // إنشاء مجلد فرعي لمشروعنا
                var projectBackupDir = Path.Combine(sqlBackupDir, "SmarterGros");

                // أنشئ المجلد إن لم يكن موجوداً (ربما يحتاج صلاحيات)
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

                // نسخ الملف إلى wwwroot/backups للتنزيل من المتصفح
                var webBackupDir = Path.Combine(_env.WebRootPath, "backups");
                Directory.CreateDirectory(webBackupDir);
                var webBackupPath = Path.Combine(webBackupDir, fileName);

                try { System.IO.File.Copy(backupPath, webBackupPath, true); }
                catch { /* الملف موجود في مجلد SQL Server */ }

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

        // POST: Restore
        [HttpPost]
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

        // GET: Download Backup
        public IActionResult DownloadBackup(string fileName)
        {
            var backupDir = Path.Combine(_env.WebRootPath, "backups");
            var filePath = Path.Combine(backupDir, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }

        // GET: List Backups
        [HttpGet]
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

        // POST: Delete All Data
        [HttpPost]
        public async Task<IActionResult> ClearDatabase()
        {
            try
            {
                // حذف البيانات مع الحفاظ على الجداول
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
        

        // ===== SUPPORT =====
        public IActionResult Support() => View();

        // ===== HELPERS =====
        private List<string> GetRoles() => new List<string>
        {
            "مدير النظام",
            "مدير المخزن",
            "مسؤول المبيعات",
            "مسؤول المشتريات",
            "مستخدم عادي"
        };
    }
}
 