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
    internal class FakeUnicornDbSet : FakeDbSet<inventory>
    {
        public override Task<inventory?> FindAsync(params object[] keyValues)
        {
            Debug.Assert(keyValues.Length == 1, "There should be only one key for Unicorn entity");
            var targetId = keyValues[0] as Guid?;

            if (targetId == null)
            {
                return Task.FromResult<inventory?>(null);
            }

            return Task.FromResult<inventory?>(this.Local.FirstOrDefault(u => u.unicorn_id == targetId));
        }
    }
}
