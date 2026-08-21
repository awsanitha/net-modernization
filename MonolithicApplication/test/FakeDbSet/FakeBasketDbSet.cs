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

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

            return this.InternalData.FirstOrDefault(u => u.basket_id == targetId);
        }

        public override ValueTask<basket?> FindAsync(params object?[]? keyValues)
            => new ValueTask<basket?>(this.Find(keyValues));

        public override ValueTask<basket?> FindAsync(object?[]? keyValues, CancellationToken cancellationToken)
            => new ValueTask<basket?>(this.Find(keyValues));
    }
}
