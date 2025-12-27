using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using magazz.Models;

namespace magazz.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        
        // Новые таблицы для нормализации
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<ProductColor> ProductColors { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка связи ApplicationUser -> Customer
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Customer)
                .WithOne()
                .HasForeignKey<ApplicationUser>(u => u.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Настройка связи Cart -> User (для авторизованных пользователей)
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройка связей Product
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.SetNull);

            // Настройка ProductSize
            modelBuilder.Entity<ProductSize>()
                .HasOne(ps => ps.Product)
                .WithMany(p => p.Sizes)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройка ProductColor
            modelBuilder.Entity<ProductColor>()
                .HasOne(pc => pc.Product)
                .WithMany(p => p.Colors)
                .HasForeignKey(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройка ProductImage
            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройка CartItem
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройка Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройка OrderItem
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройка decimal полей
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.PriceAtOrder)
                .HasColumnType("decimal(18,2)");

            // Индексы
            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.SessionId);

            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.UserId);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Price);

            // Seed-данные для категорий
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Мужское", Description = "Мужская одежда" },
                new Category { Id = 2, Name = "Женское", Description = "Женская одежда" },
                new Category { Id = 3, Name = "Аксессуары", Description = "Аксессуары и дополнения" },
                new Category { Id = 4, Name = "Обувь", Description = "Обувь" }
            );

            // Seed-данные для брендов
            modelBuilder.Entity<Brand>().HasData(
                new Brand { Id = 1, Name = "STUDIO SLOW", Description = "Малотиражные дизайнеры" },
                new Brand { Id = 2, Name = "ABRA", Description = "Современная одежда" },
                new Brand { Id = 3, Name = "MINIMAL", Description = "Минималистичный стиль" }
            );

            // Seed-данные для товаров
            modelBuilder.Entity<Product>().HasData(
                new Product 
                { 
                    Id = 1, 
                    Name = "Оверсайз свитшот", 
                    Description = "Свободный свитшот из хлопка премиум качества", 
                    Price = 5990, 
                    Stock = 20,
                    CategoryId = 1,
                    BrandId = 1
                },
                new Product 
                { 
                    Id = 2, 
                    Name = "Вязаная водолазка", 
                    Description = "Теплая водолазка из мериноса", 
                    Price = 7490, 
                    Stock = 15,
                    CategoryId = 2,
                    BrandId = 2
                },
                new Product 
                { 
                    Id = 3, 
                    Name = "Кожаная сумка", 
                    Description = "Минималистичная сумка из натуральной кожи", 
                    Price = 12990, 
                    Stock = 8,
                    CategoryId = 3,
                    BrandId = 3
                }
            );

            // Seed-данные для размеров
            modelBuilder.Entity<ProductSize>().HasData(
                // Свитшот
                new ProductSize { Id = 1, ProductId = 1, Size = "S", Stock = 5 },
                new ProductSize { Id = 2, ProductId = 1, Size = "M", Stock = 6 },
                new ProductSize { Id = 3, ProductId = 1, Size = "L", Stock = 5 },
                new ProductSize { Id = 4, ProductId = 1, Size = "XL", Stock = 4 },
                // Водолазка
                new ProductSize { Id = 5, ProductId = 2, Size = "XS", Stock = 3 },
                new ProductSize { Id = 6, ProductId = 2, Size = "S", Stock = 4 },
                new ProductSize { Id = 7, ProductId = 2, Size = "M", Stock = 5 },
                new ProductSize { Id = 8, ProductId = 2, Size = "L", Stock = 3 }
            );

            // Seed-данные для цветов
            modelBuilder.Entity<ProductColor>().HasData(
                // Свитшот
                new ProductColor { Id = 1, ProductId = 1, Color = "Черный", HexCode = "#000000", Stock = 7 },
                new ProductColor { Id = 2, ProductId = 1, Color = "Белый", HexCode = "#FFFFFF", Stock = 6 },
                new ProductColor { Id = 3, ProductId = 1, Color = "Серый", HexCode = "#808080", Stock = 7 },
                // Водолазка
                new ProductColor { Id = 4, ProductId = 2, Color = "Бежевый", HexCode = "#F5F5DC", Stock = 8 },
                new ProductColor { Id = 5, ProductId = 2, Color = "Черный", HexCode = "#000000", Stock = 7 },
                // Сумка
                new ProductColor { Id = 6, ProductId = 3, Color = "Черный", HexCode = "#000000", Stock = 4 },
                new ProductColor { Id = 7, ProductId = 3, Color = "Коричневый", HexCode = "#8B4513", Stock = 4 }
            );

            // Seed-данные для изображений (примеры URL)
            modelBuilder.Entity<ProductImage>().HasData(
                new ProductImage { Id = 1, ProductId = 1, ImageUrl = "/images/products/sweatshirt-1.jpg", AltText = "Оверсайз свитшот", DisplayOrder = 1, IsPrimary = true },
                new ProductImage { Id = 2, ProductId = 1, ImageUrl = "/images/products/sweatshirt-2.jpg", AltText = "Оверсайз свитшот вид сзади", DisplayOrder = 2, IsPrimary = false },
                new ProductImage { Id = 3, ProductId = 2, ImageUrl = "/images/products/turtleneck-1.jpg", AltText = "Вязаная водолазка", DisplayOrder = 1, IsPrimary = true },
                new ProductImage { Id = 4, ProductId = 3, ImageUrl = "/images/products/bag-1.jpg", AltText = "Кожаная сумка", DisplayOrder = 1, IsPrimary = true }
            );
        }
    }
}
