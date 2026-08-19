/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 */

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace UnicornShopLegacy.Tests
{
    internal class FakeBasketDbSet : FakeDbSet<basket>
    {
        public override Task<basket?> FindAsync(params object[] keyValues)
        {
            Debug.Assert(keyValues.Length == 1, "There should be only one key for Basket entity");
            var targetId = keyValues[0] as Guid?;

            if (targetId == null)
            {
                return Task.FromResult<basket?>(null);
            }

            return Task.FromResult<basket?>(this.Local.FirstOrDefault(u => u.basket_id == targetId));
        }
    }
}
