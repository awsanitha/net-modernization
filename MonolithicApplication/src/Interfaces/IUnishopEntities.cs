/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace UnicornShopLegacy.Interfaces
{
    public interface IUnishopEntities : IDisposable
    {
        DbSet<user> users { get; set; }

        DbSet<inventory> inventories { get; set; }

        DbSet<basket> baskets { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        void SetModified(object entity);
    }
}
