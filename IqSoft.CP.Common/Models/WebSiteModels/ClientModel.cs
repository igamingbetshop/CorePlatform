using System;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ClientModel : ApiClientSecurityModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string CurrencyId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int? Country { get; set; }
        public int? City { get; set; }
        public string CityName { get; set; }
        public int? Town { get; set; }
        public int Gender { get; set; }
        public int? BirthYear { get; set; }
        public int? BirthMonth { get; set; }
        public int? BirthDay { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string SecondName { get; set; }
        public int? DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentIssuedBy { get; set; }
        public string PassportDocumentData { get; set; }
        public string IdCardDocumentData { get; set; }
        public string UtilityBillDocumentData { get; set; }
        public string DriverLicenseDocumentData { get; set; }
        public bool IsDocumentVerified { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string MobileNumber { get; set; }
        public bool IsMobileNumberVerified { get; set; }
        public string PromoCode { get; set; }
        public string RegistrationIp { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string Token { get; set; }
        public string EmailOrMobile { get; set; }
        public bool? SendMail { get; set; }
        public bool SendSms { get; set; }
        public bool CallToPhone { get; set; }
        public bool SendPromotions { get; set; }
        public int? CategoryId { get; set; }
        public string ZipCode { get; set; }
        public string Info { get; set; }
        public string ReCaptcha { get; set; }
        public string RefId { get; set; }
        public string AgentCode { get; set; }
        public string AffiliateId { get; set; }
        public int? AffiliatePlatformId { get; set; }
        public bool? TermsConditionsAccepted { get; set; }
        public int? Citizenship { get; set; }
        public int? JobArea { get; set; }
        public string Apartment { get; set; }
        public string BuildingNumber { get; set; }
        public int? ReferralType { get; set; }
        public int CommunicationType { get; set; }
        public string CommunicationTypeValue { get; set; }
        public int? Title { get; set; }
        public string PinCode { get; set; }
    }
}