using System.ComponentModel.DataAnnotations;

namespace magazz.Models
{
    // Отдельная таблица для цветов товара
    public class ProductColor
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Цвет обязателен")]
        [StringLength(50, ErrorMessage = "Название цвета не может быть длиннее 50 символов")]
        public string Color { get; set; } = string.Empty;
        
        // HEX код цвета для отображения
        [StringLength(7, ErrorMessage = "HEX код должен быть в формате #RRGGBB")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "HEX код должен быть в формате #RRGGBB (например: #FF0000)")]
        public string? HexCode { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Количество не может быть отрицательным")]
        public int Stock { get; set; } = 0;
        
        // Foreign key
        public int ProductId { get; set; }
        
        // Navigation property
        public Product Product { get; set; } = null!;
    }
}