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
    
    public partial class Gtd_Provider_Bets
    {
        public int Id { get; set; }
        public long Date { get; set; }
        public int PartnerId { get; set; }
        public int GameProviderId { get; set; }
        public int SubProviderId { get; set; }
        public decimal BetAmount { get; set; }
        public decimal WinAmount { get; set; }
        public decimal GGR { get; set; }
        public decimal NGR { get; set; }
        public int BetsCount { get; set; }
        public int PlayersCount { get; set; }
    }
}
