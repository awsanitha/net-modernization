/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 */

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UnicornShopLegacy.Interfaces;

namespace UnicornShopLegacy
{
    /// <summary>
    /// Partial class implementing IUnishopEntities — exposes IEntitySet wrappers over EF Core DbSets.
    /// </summary>
    public partial class UnishopEntities : IUnishopEntities
    {
        private IEntitySet<user>? _users;
        private IEntitySet<inventory>? _inventories;
        private IEntitySet<basket>? _baskets;

        public IEntitySet<user> users => _users ??= new DbSetWrapper<user>(userSet);
        public IEntitySet<inventory> inventories => _inventories ??= new DbSetWrapper<inventory>(inventorySet);
        public IEntitySet<basket> baskets => _baskets ??= new DbSetWrapper<basket>(basketSet);

        public void SetModified(object entity)
        {
            this.Entry(entity).State = EntityState.Modified;
        }

        // Explicit interface implementation to satisfy Task<int> SaveChangesAsync()
        // (DbContext.SaveChangesAsync has a default CancellationToken param)
        Task<int> IUnishopEntities.SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }
    }
}
