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
    internal class FakeUserDbSet : FakeDbSet<user>
    {
        public override Task<user?> FindAsync(params object[] keyValues)
        {
            Debug.Assert(keyValues.Length == 1, "There should be only one key for User entity");
            var targetEmail = keyValues[0];

            if (targetEmail == null)
            {
                return Task.FromResult<user?>(null);
            }

            return Task.FromResult<user?>(this.Local.FirstOrDefault(u => u.email == targetEmail.ToString()));
        }
    }
}
