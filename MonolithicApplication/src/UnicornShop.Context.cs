namespace UnicornShopLegacy
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using UnicornShopLegacy.Interfaces;

    public partial class UnishopEntities : DbContext
    {
        public UnishopEntities(DbContextOptions<UnishopEntities> options)
            : base(options)
        {
        }

        // EF Core DbSet properties (required for EF to track entities)
        public virtual DbSet<basket> basketSet { get; set; }
        public virtual DbSet<user> userSet { get; set; }
        public virtual DbSet<inventory> inventorySet { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<basket>(entity =>
            {
                entity.ToTable("baskets");
                entity.HasKey(e => e.basket_id);
            });

            modelBuilder.Entity<user>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.user_id);
            });

            modelBuilder.Entity<inventory>(entity =>
            {
                entity.ToTable("inventories");
                entity.HasKey(e => e.unicorn_id);
            });
        }
    }
}
