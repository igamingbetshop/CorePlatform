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
    
    public partial class PartnerProductSetting
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public int ProductId { get; set; }
        public decimal Percent { get; set; }
        public int State { get; set; }
        public Nullable<decimal> Rating { get; set; }
        public string CategoryIds { get; set; }
        public Nullable<int> OpenMode { get; set; }
        public Nullable<bool> HasDemo { get; set; }
        public Nullable<decimal> RTP { get; set; }
        public Nullable<int> Volatility { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
    
        public virtual Partner Partner { get; set; }
        public virtual Product Product { get; set; }
    }
}
