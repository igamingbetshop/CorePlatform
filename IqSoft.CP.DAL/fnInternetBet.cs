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
    
    public partial class fnInternetBet
    {
        public long BetDocumentId { get; set; }
        public Nullable<int> BetExternalTransactionId { get; set; }
        public Nullable<int> BetInfo { get; set; }
        public int ProductId { get; set; }
        public Nullable<int> GameProviderId { get; set; }
        public Nullable<int> TicketInfo { get; set; }
        public System.DateTime BetDate { get; set; }
        public int ClientId { get; set; }
        public string ClientUserName { get; set; }
        public string ClientFirstName { get; set; }
        public string ClientLastName { get; set; }
        public decimal BetAmount { get; set; }
        public Nullable<decimal> Coefficient { get; set; }
        public string CurrencyId { get; set; }
        public Nullable<long> TicketNumber { get; set; }
        public int DeviceTypeId { get; set; }
        public int BetTypeId { get; set; }
        public Nullable<decimal> PossibleWin { get; set; }
        public Nullable<int> RoundId { get; set; }
        public int PartnerId { get; set; }
        public string ProductName { get; set; }
        public string ProviderName { get; set; }
        public Nullable<int> ClientIp { get; set; }
        public Nullable<int> Country { get; set; }
        public int ClientCategoryId { get; set; }
        public bool HasNote { get; set; }
        public bool ClientHasNote { get; set; }
        public int State { get; set; }
        public Nullable<System.DateTime> WinDate { get; set; }
        public decimal WinAmount { get; set; }
        public Nullable<int> BonusId { get; set; }
        public Nullable<System.DateTime> LastUpdateTime { get; set; }
        public long Date { get; set; }
        public Nullable<int> AffiliateReferralId { get; set; }
        public string UserPath { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<int> ProductCategoryId { get; set; }
        public Nullable<int> SubproviderId { get; set; }
        public string SubproviderName { get; set; }
        public Nullable<int> SelectionsCount { get; set; }
        public Nullable<decimal> Rake { get; set; }
        public Nullable<decimal> BonusAmount { get; set; }
        public string AffiliateId { get; set; }
        public Nullable<long> AccountId { get; set; }
    }
}
