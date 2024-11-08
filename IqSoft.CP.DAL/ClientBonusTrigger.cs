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
    
    public partial class ClientBonusTrigger
    {
        public int Id { get; set; }
        public Nullable<int> ClientId { get; set; }
        public int TriggerId { get; set; }
        public int BonusId { get; set; }
        public Nullable<decimal> SourceAmount { get; set; }
        public Nullable<System.DateTime> CreationTime { get; set; }
        public Nullable<int> BetCount { get; set; }
        public Nullable<decimal> WageringAmount { get; set; }
        public Nullable<long> ReuseNumber { get; set; }
        public Nullable<System.DateTime> LastActionDate { get; set; }
    
        public virtual Bonu Bonu { get; set; }
        public virtual Client Client { get; set; }
        public virtual TriggerSetting TriggerSetting { get; set; }
    }
}
