using System.ComponentModel.DataAnnotations;

namespace SmarterGros.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الفئة مطلوب")]
        public string Name { get; set; } = string.Empty;

        public string? SubCategory { get; set; }
        public string? Description { get; set; }
        public int ProductCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CategoryIndexViewModel
    {
        public List<CategoryViewModel> Categories { get; set; } = new();
        public int TotalCategories { get; set; }
        public int TotalProducts { get; set; }
    }
}