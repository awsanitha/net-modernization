/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 */

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace UnicornShopLegacy.Tests
{
    internal class FakeUnicornDbSet : FakeDbSet<inventory>
    {
        public override inventory? Find(params object?[]? keyValues)
        {
            if (keyValues == null || keyValues.Length != 1)
                return null;

            if (keyValues[0] is Guid targetId)
            {
                return this.Local.FirstOrDefault(u => u.unicorn_id == targetId);
            }

            return null;
        }

        public override ValueTask<inventory?> FindAsync(params object?[]? keyValues)
        {
            return new ValueTask<inventory?>(Find(keyValues));
        }

        public override ValueTask<inventory?> FindAsync(object?[]? keyValues, CancellationToken cancellationToken)
        {
            return new ValueTask<inventory?>(Find(keyValues));
        }
    }
}
