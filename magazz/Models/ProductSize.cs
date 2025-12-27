using System.ComponentModel.DataAnnotations;

namespace magazz.Models
{
    // Отдельная таблица для размеров товара
    public class ProductSize
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Размер обязателен")]
        [StringLength(10, ErrorMessage = "Размер не может быть длиннее 10 символов")]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Размер должен содержать только заглавные буквы и цифры (например: S, M, L, XL, XXL, 42, 44)")]
        public string Size { get; set; } = string.Empty;
        
        [Range(0, int.MaxValue, ErrorMessage = "Количество не может быть отрицательным")]
        public int Stock { get; set; } = 0;
        
        // Foreign key
        public int ProductId { get; set; }
        
        // Navigation property
        public Product Product { get; set; } = null!;
    }
}