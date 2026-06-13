using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmarterGros.Data;
using SmarterGros.Models;
using SmarterGros.ViewModels;

namespace SmarterGros.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Categories
                .Include(c => c.Products.Where(p => p.IsActive))
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.Name.Contains(search) ||
                                        (c.SubCategory != null && c.SubCategory.Contains(search)));

            var categories = await query.OrderBy(c => c.Name).ToListAsync();

            var vm = new CategoryIndexViewModel
            {
                Categories = categories.Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    SubCategory = c.SubCategory,
                    Description = c.Description,
                    ProductCount = c.Products.Count,
                    CreatedAt = c.CreatedAt
                }).ToList(),
                TotalCategories = categories.Count,
                TotalProducts = categories.Sum(c => c.Products.Count)
            };

            ViewBag.Search = search;
            return View(vm);
        }

        // POST: Categories/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryViewModel model)
        {
            if (string.IsNullOrEmpty(model.Name))
                return Json(new { success = false, message = "اسم الفئة مطلوب" });

            if (await _context.Categories.AnyAsync(c => c.Name == model.Name))
                return Json(new { success = false, message = "هذه الفئة موجودة مسبقاً" });

            var category = new Category
            {
                Name = model.Name,
                SubCategory = model.SubCategory,
                Description = model.Description,
                CreatedAt = DateTime.Now
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                id = category.Id,
                name = category.Name,
                message = "تم إضافة الفئة بنجاح"
            });
        }

        // GET: Categories/Get/5
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products.Where(p => p.IsActive))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return Json(new { success = false, message = "الفئة غير موجودة" });

            return Json(new
            {
                success = true,
                id = category.Id,
                name = category.Name,
                subCategory = category.SubCategory,
                description = category.Description,
                productCount = category.Products.Count
            });
        }

        // POST: Categories/Edit
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] CategoryViewModel model)
        {
            var category = await _context.Categories.FindAsync(model.Id);
            if (category == null)
                return Json(new { success = false, message = "الفئة غير موجودة" });

            if (await _context.Categories.AnyAsync(c => c.Name == model.Name && c.Id != model.Id))
                return Json(new { success = false, message = "هذا الاسم مستخدم من فئة أخرى" });

            category.Name = model.Name;
            category.SubCategory = model.SubCategory;
            category.Description = model.Description;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "تم تعديل الفئة بنجاح" });
        }

        // POST: Categories/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return Json(new { success = false, message = "الفئة غير موجودة" });

            if (category.Products.Any(p => p.IsActive))
                return Json(new
                {
                    success = false,
                    message = $"لا يمكن حذف هذه الفئة لأنها تحتوي على {category.Products.Count(p => p.IsActive)} منتج"
                });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "تم حذف الفئة بنجاح" });
        }

        // GET: Categories/GetProducts/5
        [HttpGet]
        public async Task<IActionResult> GetProducts(int id)
        {
            var products = await _context.Products
                .Where(p => p.CategoryId == id && p.IsActive)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Reference,
                    p.StockQuantity,
                    p.RetailPriceTTC,
                    p.ImagePath
                })
                .ToListAsync();

            return Json(new { success = true, products });
        }
    }
}