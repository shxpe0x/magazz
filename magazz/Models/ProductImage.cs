using System.ComponentModel.DataAnnotations;

namespace magazz.Models
{
    // Отдельная таблица для изображений товара
    public class ProductImage
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;
        
        // Альтернативный текст для изображения
        [StringLength(200)]
        public string? AltText { get; set; }
        
        // Порядок отображения
        public int DisplayOrder { get; set; } = 0;
        
        // Является ли главным изображением
        public bool IsPrimary { get; set; } = false;
        
        // Foreign key
        public int ProductId { get; set; }
        
        // Navigation property
        public Product Product { get; set; } = null!;
    }
}