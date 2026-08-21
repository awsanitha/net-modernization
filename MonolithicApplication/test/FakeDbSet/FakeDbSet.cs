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
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace UnicornShopLegacy.Tests
{
    /// <summary>
    /// In-memory fake DbSet for unit testing with EF Core.
    /// Backed by an in-memory List&lt;T&gt;; LINQ queries run against that list.
    /// </summary>
    internal class FakeDbSet<T> : DbSet<T>, IQueryable<T>
        where T : class
    {
        private readonly List<T> _data = new List<T>();

        /// <summary>Access the underlying list from derived fake-set classes.</summary>
        protected List<T> InternalData => _data;

        // ── IQueryable routing ──────────────────────────────────────────────────────
        // EF Core's abstract AsQueryable() is the single hook point; overriding it
        // makes all IQueryable/IEnumerable surface area work against our list.
        public override IQueryable<T> AsQueryable() => _data.AsQueryable();

        // IAsyncEnumerable is not used by our unit tests; throw to surface accidental use.
        public override IAsyncEnumerable<T> AsAsyncEnumerable()
            => throw new NotSupportedException("Async enumeration is not supported by FakeDbSet.");

        // ── Mutating operations ─────────────────────────────────────────────────────
        public override EntityEntry<T> Add(T entity)
        {
            _data.Add(entity);
            return null!; // return value is never used in the tested code
        }

        public override void AddRange(IEnumerable<T> entities)
        {
            _data.AddRange(entities);
        }

        public override void AddRange(params T[] entities)
        {
            _data.AddRange(entities);
        }

        public override EntityEntry<T> Remove(T entity)
        {
            _data.Remove(entity);
            return null!;
        }

        public override void RemoveRange(IEnumerable<T> entities)
        {
            foreach (var e in entities.ToList())
                _data.Remove(e);
        }

        public override void RemoveRange(params T[] entities)
        {
            foreach (var e in entities)
                _data.Remove(e);
        }

        public override EntityEntry<T> Update(T entity)
        {
            // No-op for in-memory test fake
            return null!;
        }

        public override void UpdateRange(IEnumerable<T> entities) { }
        public override void UpdateRange(params T[] entities) { }

        public override EntityEntry<T> Attach(T entity) => null!;
        public override void AttachRange(IEnumerable<T> entities) { }
        public override void AttachRange(params T[] entities) { }

        // ── Find / FindAsync ─────────────────────────────────────────────────────────
        // Derived classes (FakeUnicornDbSet, etc.) must override these.
        public override T? Find(params object?[]? keyValues)
            => throw new NotImplementedException("Override Find in a derived FakeDbSet.");

        public override ValueTask<T?> FindAsync(params object?[]? keyValues)
            => new ValueTask<T?>(Find(keyValues));

        public override ValueTask<T?> FindAsync(object?[]? keyValues, CancellationToken cancellationToken)
            => new ValueTask<T?>(Find(keyValues));

        // ── Local view (not used in unit tests; returns null to avoid EF Core internals) ──
        public override LocalView<T> Local => throw new NotSupportedException(
            "Local is not supported on FakeDbSet; use InternalData instead.");

        // ── IEntityType from DbSet<T>.EntityType ────────────────────────────────────
        // Required abstract member; not used in unit tests.
        public override IEntityType EntityType => throw new NotSupportedException(
            "EntityType is not supported on FakeDbSet.");

        // ── IQueryable<T> explicit interface members ─────────────────────────────────
        // These delegate to AsQueryable() so LINQ works correctly.
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();
        Type IQueryable.ElementType => _data.AsQueryable().ElementType;
        Expression IQueryable.Expression => _data.AsQueryable().Expression;
        IQueryProvider IQueryable.Provider => _data.AsQueryable().Provider;
    }
}
