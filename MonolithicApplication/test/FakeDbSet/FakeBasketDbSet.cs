/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 */

#nullable enable

using System;
using System.Diagnostics;
using System.Linq;

namespace UnicornShopLegacy.Tests
{
    internal class FakeBasketDbSet : FakeDbSet<basket>
    {
        public override basket? Find(params object?[]? keyValues)
        {
            Debug.Assert(keyValues != null && keyValues.Length == 1, "There should be only one key for Basket entity");
            var targetId = keyValues![0] as Guid?;

            if (targetId == null)
            {
                return null;
            }

            var data = this.Local;
            return data.FirstOrDefault(u => u.basket_id == targetId);
        }

        public override System.Threading.Tasks.ValueTask<basket?> FindAsync(params object?[]? keyValues)
        {
            return new System.Threading.Tasks.ValueTask<basket?>(this.Find(keyValues));
        }
    }
}
