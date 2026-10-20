using Microsoft.EntityFrameworkCore;
using PCRadar.Models;

namespace PCRadar.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<ProductStore> ProductStores { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }
        public DbSet<TrackedCategory> TrackedCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProductStore>()
                .HasIndex(ps => new { ps.ProductID, ps.StoreID })
                .IsUnique();
        }
    }
}