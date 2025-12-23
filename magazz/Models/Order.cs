using System.ComponentModel.DataAnnotations;

namespace magazz.Models
{
    public class Order
    {
        public int Id { get; set; }
        
        public string OrderNumber { get; set; } = string.Empty;
        
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        [Required]
        public decimal TotalAmount { get; set; }
        
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        
        [StringLength(500)]
        public string? DeliveryAddress { get; set; }
        
        [StringLength(1000)]
        public string? Note { get; set; }
        
        // Foreign key
        public int CustomerId { get; set; }
        
        // Navigation properties
        public Customer Customer { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
    
    public class OrderItem
    {
        public int Id { get; set; }
        
        public int Quantity { get; set; }
        
        public decimal PriceAtOrder { get; set; }
        
        public string? SelectedSize { get; set; }
        
        public string? SelectedColor { get; set; }
        
        // Foreign keys
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        
        // Navigation properties
        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
    
    public enum OrderStatus
    {
        Pending,        // Ожидает обработки
        Processing,     // В обработке
        Shipped,        // Отправлен
        Delivered,      // Доставлен
        Cancelled       // Отменен
    }
}
