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
    
    public partial class ClientPaymentInfo
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string AccountNickName { get; set; }
        public string ClientFullName { get; set; }
        public string CardNumber { get; set; }
        public Nullable<System.DateTime> CardExpireDate { get; set; }
        public string BankName { get; set; }
        public string BankIBAN { get; set; }
        public string BranchName { get; set; }
        public string BankAccountNumber { get; set; }
        public int Type { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
        public string WalletNumber { get; set; }
        public Nullable<int> PartnerPaymentSystemId { get; set; }
        public Nullable<int> State { get; set; }
        public Nullable<int> BankAccountType { get; set; }
    
        public virtual Client Client { get; set; }
        public virtual PartnerPaymentSetting PartnerPaymentSetting { get; set; }
    }
}
