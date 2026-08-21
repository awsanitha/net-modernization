/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 */

namespace UnicornShopLegacy
{
    using System;
    using Microsoft.EntityFrameworkCore;

    public partial class UnishopEntities : DbContext
    {
        public UnishopEntities()
        {
        }

        public UnishopEntities(DbContextOptions<UnishopEntities> options)
            : base(options)
        {
        }

        public virtual DbSet<basket> baskets { get; set; } = null!;

        public virtual DbSet<user> users { get; set; } = null!;

        public virtual DbSet<inventory> inventories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<basket>(entity =>
            {
                entity.HasKey(e => e.basket_id);
                entity.ToTable("basket");
            });

            modelBuilder.Entity<user>(entity =>
            {
                entity.HasKey(e => e.user_id);
                entity.ToTable("app_user");
            });

            modelBuilder.Entity<inventory>(entity =>
            {
                entity.HasKey(e => e.unicorn_id);
                entity.ToTable("inventory");
            });

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    @"Data Source=db.unishop.local,1433\unishop;initial catalog=Unishop;User ID=admin;Password=dMdLgX6sZoXmOU2rnWTS;");
            }
        }
    }
}
