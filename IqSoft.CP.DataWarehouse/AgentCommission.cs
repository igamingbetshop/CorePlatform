//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IqSoft.CP.DataWarehouse
{
    using System;
    using System.Collections.Generic;
    
    public partial class AgentCommission
    {
        public int Id { get; set; }
        public Nullable<int> AgentId { get; set; }
        public int ProductId { get; set; }
        public Nullable<decimal> Percent { get; set; }
        public string TurnoverPercent { get; set; }
        public Nullable<int> ClientId { get; set; }
    }
}
