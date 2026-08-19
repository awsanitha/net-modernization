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
    internal class FakeUserDbSet : FakeDbSet<user>
    {
        public override user? Find(params object?[]? keyValues)
        {
            Debug.Assert(keyValues != null && keyValues.Length == 1, "There should be only one key for User entity");
            var targetEmail = keyValues![0];

            if (targetEmail == null)
            {
                return null;
            }

            var data = this.Local;
            return data.FirstOrDefault(u => u.email == targetEmail.ToString());
        }

        public override System.Threading.Tasks.ValueTask<user?> FindAsync(params object?[]? keyValues)
        {
            return new System.Threading.Tasks.ValueTask<user?>(this.Find(keyValues));
        }
    }
}
