namespace magazz.Models
{
    public class Cart
    {
        public int Id { get; set; }
        
        // Session ID для анонимных пользователей
        public string? SessionId { get; set; }
        
        // User ID для авторизованных пользователей
        public string? UserId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public ApplicationUser? User { get; set; }
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
    
    public class CartItem
    {
        public int Id { get; set; }
        
        public int Quantity { get; set; } = 1;
        
        public string? SelectedSize { get; set; }
        
        public string? SelectedColor { get; set; }
        
        // Foreign keys
        public int CartId { get; set; }
        public int ProductId { get; set; }
        
        // Navigation properties
        public Cart Cart { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
