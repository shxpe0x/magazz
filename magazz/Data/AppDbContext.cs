using Microsoft.EntityFrameworkCore;
using magazz.Models;

namespace magazz.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка связей
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

            // Настройка типа decimal для Price
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            // Сеед-данные для категорий
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Мужское", Description = "Мужская одежда" },
                new Category { Id = 2, Name = "Женское", Description = "Женская одежда" },
                new Category { Id = 3, Name = "Аксессуары", Description = "Аксессуары и дополнения" },
                new Category { Id = 4, Name = "Обувь", Description = "Обувь" }
            );
        }
    }
}
