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
    
    public partial class fnProfitByClientProduct
    {
        public int ClientId { get; set; }
        public Nullable<int> AgentId { get; set; }
        public int ProductId { get; set; }
        public Nullable<int> SelectionsCount { get; set; }
        public Nullable<decimal> TotalBetAmount { get; set; }
        public Nullable<decimal> TotalWinAmount { get; set; }
        public Nullable<int> TotalBetsCount { get; set; }
        public Nullable<int> TotalUnsettledBetsCount { get; set; }
        public Nullable<int> TotalDeletedBetsCount { get; set; }
    }
}
