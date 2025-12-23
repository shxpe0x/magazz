using System.ComponentModel.DataAnnotations;

namespace magazz.Models
{
    public class Customer
    {
        public int Id { get; set; }
        
        [StringLength(100)]
        public string? FirstName { get; set; }
        
        [StringLength(100)]
        public string? LastName { get; set; }
        
        [Required(ErrorMessage = "Электронная почта обязательна")]
        [EmailAddress(ErrorMessage = "Некорректная электронная почта")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Phone(ErrorMessage = "Некорректный номер телефона")]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        [StringLength(500)]
        public string? Address { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation property
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
