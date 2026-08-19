/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 */

using Microsoft.EntityFrameworkCore;
using UnicornShopLegacy.Interfaces;

namespace UnicornShopLegacy
{
    public partial class UnishopEntities : DbContext, IUnishopEntities
    {
        public UnishopEntities(DbContextOptions<UnishopEntities> options)
            : base(options)
        {
        }

        public virtual DbSet<basket> baskets { get; set; }
        public virtual DbSet<user> users { get; set; }
        public virtual DbSet<inventory> inventories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<basket>().ToTable("basket");
            modelBuilder.Entity<user>().ToTable("app_user");
            modelBuilder.Entity<inventory>().ToTable("inventory");
        }
    }
}
