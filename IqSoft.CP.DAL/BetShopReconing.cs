//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IqSoft.CP.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class BetShopReconing
    {
        public long Id { get; set; }
        public int BetShopId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public decimal BetShopAvailiableBalance { get; set; }
        public long DocumentId { get; set; }
        public string CurrencyId { get; set; }
        public System.DateTime CreationTime { get; set; }
    
        public virtual User User { get; set; }
        public virtual BetShop BetShop { get; set; }
        public virtual Currency Currency { get; set; }
    }
}
