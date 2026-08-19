namespace UnicornShopLegacy
{
    using System;
    using System.Collections.Generic;

    public partial class user
    {
        public System.Guid user_id { get; set; }
        public string? email { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set; }
        public string? password { get; set; }
    }
}
