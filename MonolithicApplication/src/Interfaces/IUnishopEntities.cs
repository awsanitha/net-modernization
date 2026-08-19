/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace UnicornShopLegacy.Interfaces
{
    public interface IUnishopEntities : IDisposable
    {
        IEntitySet<user> users { get; }

        IEntitySet<inventory> inventories { get; }

        IEntitySet<basket> baskets { get; }

        EntityEntry Entry(object entity);

        Task<int> SaveChangesAsync();

        void SetModified(object entity);
    }

    /// <summary>
    /// Testable abstraction over DbSet&lt;T&gt; that supports querying and CRUD.
    /// </summary>
    public interface IEntitySet<T> : IQueryable<T>
        where T : class
    {
        T Add(T entity);
        T Remove(T entity);
        IQueryable<T> AsQueryable();
        Task<T?> FindAsync(params object[] keyValues);
        void AddRange(System.Collections.Generic.IEnumerable<T> entities);
    }
}
