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
    
    public partial class fnCorrection
    {
        public long AccountId { get; set; }
        public int AccountTypeId { get; set; }
        public long Id { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyId { get; set; }
        public int State { get; set; }
        public string Info { get; set; }
        public Nullable<int> ClientId { get; set; }
        public string ClientUserName { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<int> Creator { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
        public string OperationTypeName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public Nullable<bool> HasNote { get; set; }
        public Nullable<long> Date { get; set; }
        public Nullable<int> PartnerId { get; set; }
        public Nullable<int> AffiliateReferralId { get; set; }
        public Nullable<int> DocumentTypeId { get; set; }
        public Nullable<int> ProductId { get; set; }
        public string ProductNickName { get; set; }
        public string AffiliateId { get; set; }
    }
}
