namespace UnicornShopLegacy
{
    using System;
    using System.Collections.Generic;

    public partial class inventory
    {
        public System.Guid unicorn_id { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public decimal? price { get; set; }
        public string? image { get; set; }
        public System.DateTime? date_create { get; set; }
    }
}
