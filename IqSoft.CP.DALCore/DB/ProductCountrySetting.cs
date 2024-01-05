namespace IqSoft.CP.DAL
{
    using System;
    using System.Collections.Generic;

    public partial class ProductCountrySetting
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Nullable<int> PartnerId { get; set; }
        public int CountryId { get; set; }
        public int Type { get; set; }

        public virtual Partner Partner { get; set; }
        public virtual Product Product { get; set; }
        public virtual Region Region { get; set; }
    }
}