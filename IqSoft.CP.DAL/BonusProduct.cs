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
    
    public partial class BonusProduct
    {
        public int Id { get; set; }
        public int BonusId { get; set; }
        public int ProductId { get; set; }
        public Nullable<decimal> Percent { get; set; }
        public Nullable<int> Count { get; set; }
        public Nullable<decimal> Lines { get; set; }
        public Nullable<decimal> Coins { get; set; }
        public Nullable<decimal> CoinValue { get; set; }
        public string BetValues { get; set; }
    
        public virtual Bonu Bonu { get; set; }
        public virtual Product Product { get; set; }
    }
}
