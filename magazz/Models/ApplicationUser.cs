using Microsoft.AspNetCore.Identity;

namespace magazz.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Связь с Customer - один User = один Customer
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        
        // Дополнительные поля при необходимости
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
    }
}