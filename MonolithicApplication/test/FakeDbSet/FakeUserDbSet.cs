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
    internal class FakeUserDbSet : FakeDbSet<user>
    {
        public override user? Find(params object?[]? keyValues)
        {
            if (keyValues == null || keyValues.Length != 1)
                return null;

            var targetEmail = keyValues[0];

            if (targetEmail == null)
            {
                return null;
            }

            var data = this.Local;
            return data.FirstOrDefault(u => u.email == targetEmail.ToString());
        }

        public override ValueTask<user?> FindAsync(params object?[]? keyValues)
        {
            return new ValueTask<user?>(Find(keyValues));
        }

        public override ValueTask<user?> FindAsync(object?[]? keyValues, CancellationToken cancellationToken)
        {
            return new ValueTask<user?>(Find(keyValues));
        }
    }
}
