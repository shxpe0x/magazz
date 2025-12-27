using System.ComponentModel.DataAnnotations;

namespace magazz.Models
{
    // Отдельная таблица для цветов товара
    public class ProductColor
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Color { get; set; } = string.Empty;
        
        // HEX код цвета для отображения
        [StringLength(7)]
        public string? HexCode { get; set; }
        
        public int Stock { get; set; } = 0;
        
        // Foreign key
        public int ProductId { get; set; }
        
        // Navigation property
        public Product Product { get; set; } = null!;
    }
}