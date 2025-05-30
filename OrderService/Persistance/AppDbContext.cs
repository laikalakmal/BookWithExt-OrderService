using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Persistance
{
    public class AppDbContext : DbContext
    {

        //
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<Order> Orders { get; set; } 
        public DbSet<OrderItem> OrderItems { get; set; }



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




            // Configure order entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasMany(e => e.Items)
                    .WithOne()
                    .HasForeignKey("OrderId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure OrderItem entity
            modelBuilder.Entity<OrderItem>(entity =>
            {
                
                entity.HasKey(e => e.Id);

                entity.Property<Guid>("OrderId").IsRequired();
                
                // Configure properties
                entity.Property(e => e.productId).IsRequired();
                entity.Property(e => e.quantity).IsRequired();
                entity.Property(e => e.priceAtPurchase).HasColumnType("decimal(18,2)").IsRequired();
                entity.OwnsOne(e => e.PurchaseResponse, builder => 
                {
                    builder.Ignore(p => p.AdditionalData);
                });
            });
        }
    }
}
