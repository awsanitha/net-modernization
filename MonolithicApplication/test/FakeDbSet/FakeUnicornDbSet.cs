/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 */

#nullable enable

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace UnicornShopLegacy.Tests
{
    internal class FakeUnicornDbSet : FakeDbSet<inventory>
    {
        public override inventory? Find(params object?[]? keyValues)
        {
            Debug.Assert(keyValues != null && keyValues.Length == 1, "There should be only one key for Unicorn entity");

            var targetId = keyValues![0] as Guid?;

            if (targetId == null)
            {
                return null;
            }

            return this.Local.FirstOrDefault(u => u.unicorn_id == targetId);
        }

        public override System.Threading.Tasks.ValueTask<inventory?> FindAsync(params object?[]? keyValues)
        {
            return new System.Threading.Tasks.ValueTask<inventory?>(this.Find(keyValues));
        }
    }
}
