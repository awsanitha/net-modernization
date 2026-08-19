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
    internal class FakeBasketDbSet : FakeDbSet<basket>
    {
        public override basket? Find(params object?[]? keyValues)
        {
            if (keyValues == null || keyValues.Length != 1)
                return null;

            if (keyValues[0] is Guid targetId)
            {
                return this.Local.FirstOrDefault(u => u.basket_id == targetId);
            }

            return null;
        }

        public override ValueTask<basket?> FindAsync(params object?[]? keyValues)
        {
            return new ValueTask<basket?>(Find(keyValues));
        }

        public override ValueTask<basket?> FindAsync(object?[]? keyValues, CancellationToken cancellationToken)
        {
            return new ValueTask<basket?>(Find(keyValues));
        }
    }
}
