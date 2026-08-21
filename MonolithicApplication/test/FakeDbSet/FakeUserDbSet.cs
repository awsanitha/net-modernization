/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this
 * software and associated documentation files (the "Software"), to deal in the Software
 * without restriction, including without limitation the rights to use, copy, modify,
 * merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Diagnostics;
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
            Debug.Assert(keyValues != null && keyValues.Length == 1, "There should be only one key for User entity");
            var targetEmail = keyValues![0];

            if (targetEmail == null)
            {
                return null;
            }

            return this.InternalData.FirstOrDefault(u => u.email == (string)targetEmail);
        }

        public override ValueTask<user?> FindAsync(params object?[]? keyValues)
            => new ValueTask<user?>(this.Find(keyValues));

        public override ValueTask<user?> FindAsync(object?[]? keyValues, CancellationToken cancellationToken)
            => new ValueTask<user?>(this.Find(keyValues));
    }
}
