using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllClient
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string CurrencyId { get; set; }
        public string UserName { get; set; }
        public int PartnerId { get; set; }
        public int? Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public int State { get; set; }
        public int CategoryId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string SecondName { get; set; }
        public string SecondSurname { get; set; }
        public string DocumentNumber { get; set; }
        public int? DocumentType { get; set; }
        public string DocumentIssuedBy { get; set; }
        public bool IsDocumentVerified { get; set; }
        public string Address { get; set; }
        public bool SendMail { get; set; }
        public bool SendSms { get; set; }
        public bool SendPromotions { get; set; }
        public string MobileNumber { get; set; }
        public bool IsMobileNumberVerified { get; set; }
        public string LanguageId { get; set; }
        public DateTime CreationTime { get; set; }
        public string CategoryName { get; set; }
        public int? AffiliateReferralId { get; set; }
        public int RegionId { get; set; }
        public string City { get; set; }
        public int? BetShopId { get; set; }
        public int? UserId { get; set; }
        public long? LastSessionId { get; set; }
        public string Info { get; set; }
        public string PasswordHash { get; set; }
        public int Salt { get; set; }
        public string CurrencySymbol { get; set; }
        public string ZipCode { get; set; }
        public string EmailOrMobile
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Email))
                    return Email;
                return MobileNumber;
            }
        }
        public int? Citizenship { get; set; }
        public int? JobArea { get; set; }
        public string Apartment { get; set; }
        public string BuildingNumber { get; set; }
        public string AlternativeDomain { get; set; }
        public string AlternativeDomainMessage { get; set; }
    }
}