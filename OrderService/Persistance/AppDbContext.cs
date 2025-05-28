using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Persistance
{
    public class AppDbContext : DbContext
    {
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Cart entity
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(e => e.Id);
               
            });
            
            // Configure CartItem entity
            modelBuilder.Entity<CartItem>(entity =>
            {
                // Composite key consisting of CartId and productId
                entity.HasKey(e => e.Id);
                
                // Configure properties
                entity.Property(e => e.productId).IsRequired();
                entity.Property(e => e.quantity).IsRequired();
                entity.Property(e => e.priceAtPurchase).HasColumnType("decimal(18,2)").IsRequired();
            });
        }
    }
}
