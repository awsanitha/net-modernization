/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this
 * software and associated documentation files (the "Software"), to deal in the Software
 * without restriction, including without limitation the rights to use, copy, modify,
 * merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

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
    /// <summary>
    /// In-memory fake DbSet for unit testing without a real database.
    /// Inherits from DbSet[T] (EF Core) and implements IQueryable via a backing List.
    /// </summary>
    internal class FakeDbSet<T> : DbSet<T>, IQueryable<T>, IAsyncEnumerable<T>
        where T : class
    {
        private readonly List<T> _data = new();

        // EF Core abstract property — not used in tests but must be implemented
        public override IEntityType EntityType => throw new NotImplementedException("EntityType is not supported in FakeDbSet");

        public new List<T> Local => _data;

        IQueryProvider IQueryable.Provider => _data.AsQueryable().Provider;
        Expression IQueryable.Expression => _data.AsQueryable().Expression;
        Type IQueryable.ElementType => _data.AsQueryable().ElementType;

        public override T? Find(params object?[]? keyValues)
            => throw new NotImplementedException("Derive from FakeDbSet<T> and override Find");

        public override ValueTask<T?> FindAsync(params object?[]? keyValues)
            => new(Find(keyValues));

        public override ValueTask<T?> FindAsync(object?[]? keyValues, CancellationToken cancellationToken)
            => new(Find(keyValues));

        public override EntityEntry<T> Add(T item)
        {
            _data.Add(item);
            return null!;
        }

        public override EntityEntry<T> Remove(T item)
        {
            _data.Remove(item);
            return null!;
        }

        public override EntityEntry<T> Attach(T item)
            => null!;

        public override void AddRange(IEnumerable<T> entities)
            => _data.AddRange(entities);

        public override void AddRange(params T[] entities)
            => _data.AddRange(entities);

        public override void RemoveRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities.ToList())
            {
                _data.Remove(entity);
            }
        }

        public override void RemoveRange(params T[] entities)
        {
            foreach (var entity in entities)
            {
                _data.Remove(entity);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _data.GetEnumerator();

        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken)
            => new AsyncEnumerator(_data.GetEnumerator());

        private sealed class AsyncEnumerator : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;
            public AsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
            public T Current => _inner.Current;
            public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());
            public ValueTask DisposeAsync() { _inner.Dispose(); return ValueTask.CompletedTask; }
        }
    }
}
