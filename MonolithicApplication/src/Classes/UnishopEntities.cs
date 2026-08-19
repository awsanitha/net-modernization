/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 */

using Microsoft.EntityFrameworkCore;
using UnicornShopLegacy.Interfaces;

namespace UnicornShopLegacy
{
    /// <summary>
    /// Partial class only to implement IUnishopEntities interface and provide mockable method instead of Entry()
    /// </summary>
    public partial class UnishopEntities : IUnishopEntities
    {
        public void SetModified(object entity)
        {
            this.Entry(entity).State = EntityState.Modified;
        }
    }
}
