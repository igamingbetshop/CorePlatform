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
    
    public partial class CurrencyRate
    {
        public int Id { get; set; }
        public string CurrencyId { get; set; }
        public decimal RateBefore { get; set; }
        public decimal RateAfter { get; set; }
        public long SessionId { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
    
        public virtual Currency Currency { get; set; }
        public virtual UserSession UserSession { get; set; }
    }
}