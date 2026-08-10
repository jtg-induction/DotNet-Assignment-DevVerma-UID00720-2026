using System.Data.Entity;
using Assignment3.Models;

namespace Assignment3.Data
{
    public class FoodOrderingContext : DbContext
    {
        public FoodOrderingContext() : base("FoodOrderingDB")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<RestaurantOwner> RestaurantOwners { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Composite Primary Key
            modelBuilder.Entity<RestaurantOwner>()
                .HasKey(ro => new { ro.UserId, ro.RestaurantId });

            // Decimal Precision
            modelBuilder.Entity<User>()
                .Property(u => u.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<MenuItem>()
                .Property(m => m.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            // User -> Addresses (One-to-Many)
            modelBuilder.Entity<Address>()
                .HasOptional(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .WillCascadeOnDelete(false);

            // Restaurant -> MenuItems (One-to-Many)
            modelBuilder.Entity<MenuItem>()
                .HasRequired(m => m.Restaurant)
                .WithMany(r => r.MenuItems)
                .HasForeignKey(m => m.RestaurantId)
                .WillCascadeOnDelete(false);

            // User -> Orders (One-to-Many)
            modelBuilder.Entity<Order>()
                .HasRequired(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .WillCascadeOnDelete(false);

            // Restaurant -> Orders (One-to-Many)
            modelBuilder.Entity<Order>()
                .HasRequired(o => o.Restaurant)
                .WithMany(r => r.Orders)
                .HasForeignKey(o => o.RestaurantId)
                .WillCascadeOnDelete(false);

            // Order -> OrderItems (One-to-Many)
            modelBuilder.Entity<OrderItem>()
                .HasRequired(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .WillCascadeOnDelete(false);

            // MenuItem -> OrderItems (One-to-Many)
            modelBuilder.Entity<OrderItem>()
                .HasRequired(oi => oi.MenuItem)
                .WithMany(m => m.OrderItems)
                .HasForeignKey(oi => oi.MenuItemId)
                .WillCascadeOnDelete(false);

            // User -> RefreshTokens (One-to-Many)
            modelBuilder.Entity<RefreshToken>()
                .HasRequired(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .WillCascadeOnDelete(false);

            // User -> RestaurantOwners (One-to-Many)
            modelBuilder.Entity<RestaurantOwner>()
                .HasRequired(ro => ro.User)
                .WithMany(u => u.RestaurantOwners)
                .HasForeignKey(ro => ro.UserId)
                .WillCascadeOnDelete(false);

            // Restaurant -> RestaurantOwners (One-to-Many)
            modelBuilder.Entity<RestaurantOwner>()
                .HasRequired(ro => ro.Restaurant)
                .WithMany(r => r.RestaurantOwners)
                .HasForeignKey(ro => ro.RestaurantId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}
