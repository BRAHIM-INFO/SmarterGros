using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmarterGros.Data;
using SmarterGros.Models;

namespace SmarterGros.Controllers
{
    [Authorize]
    public class StockMovementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StockMovementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? type, int? productId, DateTime? from, DateTime? to)
        {
            var query = _context.StockMovements.Include(m => m.Product).AsQueryable();
            if (!string.IsNullOrEmpty(type)) query = query.Where(m => m.MovementType == type);
            if (productId.HasValue) query = query.Where(m => m.ProductId == productId.Value);
            if (from.HasValue) query = query.Where(m => m.MovementDate >= from.Value);
            if (to.HasValue) query = query.Where(m => m.MovementDate <= to.Value.AddDays(1));
            var movements = await query.OrderByDescending(m => m.MovementDate).ToListAsync();
            ViewBag.Products = await _context.Products.Where(p => p.IsActive).ToListAsync();
            return View(movements);
        }

        [HttpPost]
        public async Task<IActionResult> AddMovement([FromBody] StockMovementViewModel model)
        {
            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null) return Json(new { success = false, message = "المنتج غير موجود" });

            var oldQty = product.StockQuantity;

            if (model.MovementType == "إدخال")
                product.StockQuantity += model.Quantity;
            else if (model.MovementType == "إخراج")
            {
                if (product.StockQuantity < model.Quantity)
                    return Json(new { success = false, message = "الكمية غير كافية" });
                product.StockQuantity -= model.Quantity;
            }
            else if (model.MovementType == "تعديل")
                product.StockQuantity = model.Quantity;

            var movement = new StockMovement
            {
                ProductId = model.ProductId,
                MovementType = model.MovementType,
                Quantity = model.Quantity,
                QuantityBefore = oldQty,
                QuantityAfter = product.StockQuantity,
                Reason = model.Reason,
                Notes = model.Notes,
                UserName = User.Identity?.Name,
                MovementDate = DateTime.Now
            };

            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }

    public class StockMovementViewModel
    {
        public int ProductId { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
    }
}