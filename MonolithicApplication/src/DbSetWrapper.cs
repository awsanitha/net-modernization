/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UnicornShopLegacy.Interfaces;

namespace UnicornShopLegacy
{
    /// <summary>
    /// Wraps EF Core DbSet&lt;T&gt; to implement IEntitySet&lt;T&gt; for testability.
    /// </summary>
    public class DbSetWrapper<T> : IEntitySet<T>
        where T : class
    {
        private readonly DbSet<T> _dbSet;

        public DbSetWrapper(DbSet<T> dbSet)
        {
            _dbSet = dbSet;
        }

        public T Add(T entity)
        {
            _dbSet.Add(entity);
            return entity;
        }

        public T Remove(T entity)
        {
            _dbSet.Remove(entity);
            return entity;
        }

        public IQueryable<T> AsQueryable() => _dbSet.AsQueryable();

        public async Task<T?> FindAsync(params object[] keyValues)
            => await _dbSet.FindAsync(keyValues);

        public void AddRange(IEnumerable<T> entities)
            => _dbSet.AddRange(entities);

        // IQueryable<T> implementation delegates to the DbSet
        public Type ElementType => ((IQueryable<T>)_dbSet).ElementType;
        public Expression Expression => ((IQueryable<T>)_dbSet).Expression;
        public IQueryProvider Provider => ((IQueryable<T>)_dbSet).Provider;

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_dbSet).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dbSet).GetEnumerator();
    }
}
