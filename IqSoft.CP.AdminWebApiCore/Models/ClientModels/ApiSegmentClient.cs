using System;

namespace IqSoft.CP.AdminWebApi.Models.ClientModels
{
    public class ApiSegmentClient
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string CurrencyId { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public int Salt { get; set; }
        public int PartnerId { get; set; }
        public int Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public bool SendMail { get; set; }
        public bool SendSms { get; set; }
        public bool CallToPhone { get; set; }
        public bool SendPromotions { get; set; }
        public int State { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int RegionId { get; set; }
        public string Info { get; set; }
        public string ZipCode { get; set; }
        public string RegistrationIp { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentIssuedBy { get; set; }
        public bool IsDocumentVerified { get; set; }
        public string Address { get; set; }
        public string MobileNumber { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsMobileNumberVerified { get; set; }
        public bool HasNote { get; set; }
        public string LanguageId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public DateTime? FirstDepositDate { get; set; }
        public DateTime? LastDepositDate { get; set; }
        public decimal? LastDepositAmount { get; set; }
        public int CategoryId { get; set; }
        public int? DocumentType { get; set; }
        public string SecondName { get; set; }
        public string SecondSurname { get; set; }
        public int? UserId { get; set; }
        public int? SegmentId { get; set; }
    }
}