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
    
    public partial class ClientBankInfo
    {
        public int ClientId { get; set; }
        public int BankInfoId { get; set; }
        public System.DateTime LastViewDate { get; set; }
    
        public virtual PartnerBankInfo PartnerBankInfo { get; set; }
        public virtual Client Client { get; set; }
    }
}
