using System;

namespace IqSoft.CP.AdminWebApi.ClientModels.Models
{
    public class ClientModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string CurrencyId { get; set; }
        public string UserName { get; set; }
        public int PartnerId { get; set; }
        public int? Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public int Age { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentIssuedBy { get; set; }
        public string Address { get; set; }
        public string MobileNumber { get; set; }
        public bool IsMobileNumberVerified { get; set; }
        public string LanguageId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string RegistrationIp { get; set; }
        public bool SendMail { get; set; }
        public bool SendSms { get; set; }
        public int RegionId { get; set; }
        public int? CountryId { get; set; }
        public int? DistrictId { get; set; }
        public int? CityId { get; set; }
        public int? TownId { get; set; }
        public string City { get; set; }
        public int? Citizenship { get; set; }
        public int? JobArea { get; set; }
        public bool IsDocumentVerified { get; set; }
        public string PhoneNumber { get; set; }
        public bool CallToPhone { get; set; }
        public bool SendPromotions { get; set; }
        public int State { get; set; }
        public int CategoryId { get; set; }
        public int? UserId { get; set; }
        public string ZipCode { get; set; }
        public string Info { get; set; }
        public int? DocumentType { get; set; }
        public bool HasNote { get; set; }
        public DateTime? FirstDepositDate { get; set; }
        public DateTime? LastDepositDate { get; set; }
        public decimal? LastDepositAmount { get; set; }
        public int? BetShopId { get; set; }
        public string SecondName { get; set; }
        public string SecondSurname { get; set; }

        public string BuildingNumber { get; set; }
        public string Apartment { get; set; }
        public string AffiliateId { get; set; }
        public int? AffiliatePlatformId { get; set; }
        public string RefId { get; set; }
        public int? ReferralType { get; set; }
        public string USSDPin { get; set; }
    }
}