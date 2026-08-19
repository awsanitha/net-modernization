/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 */

namespace UnicornShopLegacy
{
    public partial class basket
    {
        public System.Guid basket_id { get; set; }
        public System.Guid user_id { get; set; }
        public System.Guid unicorn_id { get; set; }
        public System.DateTime creation_date { get; set; }
    }
}
