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
using UnicornShopLegacy.Interfaces;

namespace UnicornShopLegacy.Tests
{
    internal class FakeDbSet<T> : IEntitySet<T>
        where T : class
    {
        protected readonly List<T> data;

        public FakeDbSet()
        {
            this.data = new List<T>();
        }

        public List<T> Local => this.data;

        // IEntitySet<T> implementation
        public virtual T Add(T item)
        {
            this.data.Add(item);
            return item;
        }

        public virtual T Remove(T item)
        {
            this.data.Remove(item);
            return item;
        }

        public IQueryable<T> AsQueryable() => this.data.AsQueryable();

        public virtual Task<T?> FindAsync(params object[] keyValues)
        {
            throw new NotImplementedException("Derive from FakeDbSet<T> and override FindAsync");
        }

        public void AddRange(IEnumerable<T> entities)
        {
            this.data.AddRange(entities);
        }

        // IQueryable<T> implementation
        public Type ElementType => this.data.AsQueryable().ElementType;
        public Expression Expression => this.data.AsQueryable().Expression;
        public IQueryProvider Provider => this.data.AsQueryable().Provider;

        public IEnumerator<T> GetEnumerator() => this.data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.data.GetEnumerator();
    }
}
