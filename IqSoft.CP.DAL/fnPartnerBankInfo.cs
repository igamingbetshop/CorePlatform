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
    
    public partial class fnPartnerBankInfo
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public Nullable<int> PaymentSystemId { get; set; }
        public string NickName { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string OwnerName { get; set; }
        public string BranchName { get; set; }
        public string IBAN { get; set; }
        public string AccountNumber { get; set; }
        public string CurrencyId { get; set; }
        public bool Active { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
        public int Type { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }
    }
}
