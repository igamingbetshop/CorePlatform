﻿using IqSoft.CP.Common.Attributes;
using System;

namespace IqSoft.CP.AdminWebApi.ClientModels.Models
{
    public class fnClientModel
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string CurrencyId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [NotExcelProperty]
        public string NickName { get; set; }
        [NotExcelProperty]
        public string SecondName { get; set; }
        [NotExcelProperty]
        public string SecondSurname { get; set; }
        [NotExcelProperty]
        public int? Gender { get; set; }
        public string GenderName { get; set; }
        public string MobileNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string DocumentNumber { get; set; }
        public string ZipCode { get; set; }
        public string BirthDate { get; set; }
        public int Age { get; set; }
        public string RegionIsoCode { get; set; }
        public int? CategoryId { get; set; }
        public int? State { get; set; }
        public string StateName { get; set; }
        public int? UserId { get; set; }
        public decimal Balance { get; set; }
        public decimal GGR { get; set; }
        public decimal NETGaming { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsMobileNumberVerified { get; set; }
        public bool IsDocumentVerified { get; set; }
        public string AffiliateId { get; set; }
        public int? AffiliatePlatformId { get; set; }
        public DateTime? LastDepositDate { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastSessionDate { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string LanguageId { get; set; }
        public bool SendPromotions { get; set; }

        [NotExcelProperty]
        public bool SendMail { get; set; }

        [NotExcelProperty]
        public bool SendSms { get; set; }

        [NotExcelProperty]
        public int RegionId { get; set; }

        [NotExcelProperty]
        public string RegistrationIp { get; set; }

        [NotExcelProperty]
        public int? DocumentType { get; set; }

        [NotExcelProperty]
        public string DocumentIssuedBy { get; set; }

        [NotExcelProperty]
        public byte[] TimeStamp { get; set; }

        [NotExcelProperty]
        public bool CallToPhone { get; set; }

        [NotExcelProperty]
        public int? InformedFrom { get; set; }
        [NotExcelProperty]
        public bool HasNote { get; set; }

        [NotExcelProperty]
        public string Info { get; set; }
    }
}