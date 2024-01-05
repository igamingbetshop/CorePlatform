using System;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ApiLoginClientOutput : ApiResponseBase
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int PartnerId { get; set; }
        public int RegionId { get; set; }
        public int TownId { get; set; }
        public int CityId { get; set; }
        public string City { get; set; }
        public int DistrictId { get; set; }
        public int CountryId { get; set; }
        public int Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string DocumentNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string DocumentIssuedBy { get; set; }
        public string Address { get; set; }
        public string MobileNumber { get; set; }
        public bool IsMobileNumberVerified { get; set; }
        public string LanguageId { get; set; }
        public string RegistrationIp { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string Token { get; set; }
        public string EmailOrMobile { get; set; }
        public bool SendMail { get; set; }
        public bool SendSms { get; set; }
        public bool CallToPhone { get; set; }
        public bool SendPromotions { get; set; }
        public bool IsDocumentVerified { get; set; }
        public string ZipCode { get; set; }
        public string Info { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string WelcomeBonusActivationKey { get; set; }
        public int? Citizenship { get; set; }
        public int? JobArea { get; set; }
        public string CurrencySymbol { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? LastLogout { get; set; }
        public string LastLoginIp { get; set; }
        public int? SessionTimeLeft { get; set; }
        public decimal? SportBets { get; set; }
        public decimal? SportWins { get; set; }
        public decimal? SportProfit { get; set; }
        public bool? ResetPassword { get; set; }
        public bool? ResetNickName { get; set; }
        public bool? AcceptTermsConditions { get; set; }
        public int? DocumentExpirationStatus { get; set; }
        public string AD { get; set; }
        public string ADM { get; set; }
        public double TimeZone { get; set; }
        public string USSDPin { get; set; }
        public int? Title { get; set; }
        public int? CharacterId { get; set; }
        public bool IsTwoFactorEnabled { get; set; }
        public string IframeUrl { get; set; }
    }
}