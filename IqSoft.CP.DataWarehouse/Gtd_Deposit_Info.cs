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
    
    public partial class Gtd_Deposit_Info
    {
        public int Id { get; set; }
        public long Date { get; set; }
        public int PartnerId { get; set; }
        public int PaymentSystemId { get; set; }
        public int Status { get; set; }
        public decimal TotalAmount { get; set; }
        public int RequestsCount { get; set; }
        public int PlayersCount { get; set; }
    }
}
