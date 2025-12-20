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
        
        // Размеры через запятую (S, M, L, XL)
        public string? AvailableSizes { get; set; }
        
        // Цвета через запятую
        public string? AvailableColors { get; set; }
        
        // URL изображений через запятую
        public string? ImageUrls { get; set; }
        
        public int Stock { get; set; } = 0;
        
        public bool IsAvailable { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Foreign keys
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        
        // Navigation properties
        public Category Category { get; set; } = null!;
        public Brand? Brand { get; set; }
    }
}
