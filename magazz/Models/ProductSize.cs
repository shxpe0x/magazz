using System.ComponentModel.DataAnnotations;

namespace magazz.Models
{
    // Отдельная таблица для размеров товара
    public class ProductSize
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Size { get; set; } = string.Empty;
        
        public int Stock { get; set; } = 0;
        
        // Foreign key
        public int ProductId { get; set; }
        
        // Navigation property
        public Product Product { get; set; } = null!;
    }
}