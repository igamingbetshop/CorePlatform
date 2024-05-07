using IqSoft.CP.Common.Attributes;
using System;

namespace IqSoft.CP.AdminWebApi.Models.ClientModels
{
    public class ApiSegmentClient
    {
        public int? SegmentId { get; set; }
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string MobileNumber { get; set; }
        public bool IsMobileNumberVerified { get; set; }
        public string CurrencyId { get; set; }
        public int? Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public bool SendMail { get; set; }
        public bool SendSms { get; set; }
        public bool CallToPhone { get; set; }
        public bool SendPromotions { get; set; }
        public int State { get; set; }
        [NotExcelProperty]
        public int RegionId { get; set; }
        [NotExcelProperty]
        public string Info { get; set; }
        public string ZipCode { get; set; }
        [NotExcelProperty]
        public string RegistrationIp { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentIssuedBy { get; set; }
        public bool IsDocumentVerified { get; set; }
        public string Address { get; set; }
        [NotExcelProperty]       
        public string PhoneNumber { get; set; }
        [NotExcelProperty]
        public bool HasNote { get; set; }
        public string LanguageId { get; set; }

        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        [NotExcelProperty]
        public DateTime? FirstDepositDate { get; set; }
        public DateTime? LastDepositDate { get; set; }
        [NotExcelProperty]
        public decimal? LastDepositAmount { get; set; }
        [NotExcelProperty]
        public int CategoryId { get; set; }
        [NotExcelProperty]
        public int? DocumentType { get; set; }
        [NotExcelProperty]
        public string SecondName { get; set; }
        [NotExcelProperty]
        public string SecondSurname { get; set; }
        [NotExcelProperty]
        public int? UserId { get; set; }
        public int? AffiliatePlatformId { get; set; }
        public string AffiliateId { get; set; }
        public string AffiliateReferralId { get; set; }
    }
}