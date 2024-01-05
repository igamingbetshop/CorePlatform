using System;

namespace IqSoft.CP.AutomationTest.Models
{
    public class LoginOutput
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string CurrencyId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int PartnerId { get; set; }
        public int RegionId { get; set; }
        public int CountryId { get; set; }
        public int Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
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
        public string CurrencySymbol { get; set; }
        public string LastLogin { get; set; }
        public string LastLogout { get; set; }
        public string LastLoginIp { get; set; }
        public string ResetPassword { get; set; }
        public string ResetNickName { get; set; }
        public string AcceptTermsConditions { get; set; }
        public string DocumentExpirationStatus { get; set; }
        public string AD { get; set; }
        public string ADM { get; set; }
        public float LoginClient { get; set; }
        public int ResponseCode { get; set; }
        public string Description { get; set; }
        public string ResponseObject { get; set; }
    }

}
