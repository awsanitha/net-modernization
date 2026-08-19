namespace UnicornShopLegacy
{
    using System;
    using System.Collections.Generic;

    public partial class basket
    {
        public System.Guid basket_id { get; set; }
        public System.Guid user_id { get; set; }
        public System.Guid unicorn_id { get; set; }
        public System.DateTime creation_date { get; set; }
    }
}
