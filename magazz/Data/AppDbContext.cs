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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка связи ApplicationUser -> Customer
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Customer)
                .WithOne()
                .HasForeignKey<ApplicationUser>(u => u.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

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

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

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
                    Description = "Свободный свитшот из хлопка", 
                    Price = 5990, 
                    AvailableSizes = "S,M,L,XL",
                    AvailableColors = "Черный,Белый,Серый",
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
                    AvailableSizes = "XS,S,M,L",
                    AvailableColors = "Бежевый,Черный",
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
                    AvailableColors = "Черный,Коричневый",
                    Stock = 8,
                    CategoryId = 3,
                    BrandId = 3
                }
            );
        }
    }
}
