using magazz.Models;

namespace magazz.ViewModels
{
    public class ProductListViewModel
    {
        public PaginatedList<Product> Products { get; set; } = null!;
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Brand> Brands { get; set; } = new List<Brand>();
        
        // Текущие фильтры
        public int? CurrentCategoryId { get; set; }
        public int? CurrentBrandId { get; set; }
        public string? CurrentSearch { get; set; }
        public string? CurrentSort { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}