using System.ComponentModel.DataAnnotations;

namespace magazz.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Название товара обязательно")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Цена обязательна")]
        [Range(0.01, 1000000, ErrorMessage = "Цена должна быть больше 0")]
        public decimal Price { get; set; }
        
        // Удалены строковые поля, теперь используются отдельные таблицы
        // public string? AvailableSizes { get; set; }
        // public string? AvailableColors { get; set; }
        // public string? ImageUrls { get; set; }
        
        public int Stock { get; set; } = 0;
        
        public bool IsAvailable { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Foreign keys
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        
        // Navigation properties
        public Category Category { get; set; } = null!;
        public Brand? Brand { get; set; }
        
        // Новые коллекции для нормализованных данных
        public ICollection<ProductSize> Sizes { get; set; } = new List<ProductSize>();
        public ICollection<ProductColor> Colors { get; set; } = new List<ProductColor>();
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}