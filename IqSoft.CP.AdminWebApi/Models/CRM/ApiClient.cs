using System;

namespace IqSoft.CP.AdminWebApi.Models.CRM
{
    public class ApiClient
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string CurrencyId { get; set; }
        public long BirthDate { get; set; }
        public int? Gender { get; set; }
        public string MobileNumber { get; set; }
        public bool IsMobileNumberVerified { get; set; }
        public string AffiliateId { get; set; }
        public int? AffiliatePlatformId { get; set; }
        public string AffiliateReferralId { get; set; }
        public bool IsBonusEligible { get; set; }
        public int? CategoryId { get; set; }
        public int State { get; set; }
        public bool IsBanned { get; set; }
        public string ZipCode { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public long CreationTime { get; set; }
        public long LastUpdateTime { get; set; }
        public decimal RealBalance { get; set; }
        public decimal BonusBalance { get; set; }
        public decimal CompBalance { get; set; }
        public long? FirstDepositDate { get; set; }
        public long? LastDepositDate { get; set; }
        public long? LastLoginDate { get; set; }
    }
}