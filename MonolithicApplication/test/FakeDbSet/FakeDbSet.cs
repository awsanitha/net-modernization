/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 */

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace UnicornShopLegacy.Tests
{
    internal class FakeDbSet<T> : DbSet<T>, IQueryable<T>, IAsyncEnumerable<T>
        where T : class
    {
        private readonly List<T> data;
        private readonly IQueryable<T> queryable;

        public FakeDbSet()
        {
            this.data = new List<T>();
            this.queryable = this.data.AsQueryable();
        }

        public override EntityEntry<T> Add(T entity)
        {
            this.data.Add(entity);
            return null!;
        }

        public override EntityEntry<T> Remove(T entity)
        {
            this.data.Remove(entity);
            return null!;
        }

        public override ValueTask<T?> FindAsync(params object?[]? keyValues)
        {
            return new ValueTask<T?>(this.Find(keyValues));
        }

        public override ValueTask<T?> FindAsync(object?[]? keyValues, CancellationToken cancellationToken)
        {
            return new ValueTask<T?>(this.Find(keyValues));
        }

        public override T? Find(params object?[]? keyValues)
        {
            throw new NotImplementedException("Derive from FakeDbSet<T> and override Find");
        }

        public override IEntityType EntityType => throw new NotImplementedException();

        Type IQueryable.ElementType => queryable.ElementType;

        Expression IQueryable.Expression => queryable.Expression;

        IQueryProvider IQueryable.Provider => queryable.Provider;

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => data.GetEnumerator();

        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken)
            => new AsyncEnumeratorWrapper<T>(data.GetEnumerator());

        public new void AddRange(IEnumerable<T> entities)
        {
            this.data.AddRange(entities);
        }

        public new void RemoveRange(IEnumerable<T> entities)
        {
            foreach (var e in new List<T>(entities))
            {
                this.data.Remove(e);
            }
        }

        public new List<T> Local => this.data;
    }

    internal class AsyncEnumeratorWrapper<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> inner;

        public AsyncEnumeratorWrapper(IEnumerator<T> inner)
        {
            this.inner = inner;
        }

        public T Current => inner.Current;

        public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(inner.MoveNext());

        public ValueTask DisposeAsync()
        {
            inner.Dispose();
            return default;
        }
    }
}
