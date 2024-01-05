using IqSoft.CP.AgentWebApi.Models.ClientModels;
using IqSoft.CP.Common.Attributes;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.AgentWebApi.ClientModels
{
    public class fnClientModel
    {
        public int Id { get; set; }
        public int Level { get; set; }
        public string Email { get; set; }

        [NotExcelProperty]
        public bool IsEmailVerified { get; set; }
        public string CurrencyId { get; set; }
        public string UserName { get; set; }
        public int PartnerId { get; set; }
        public int Gender { get; set; }
        public DateTime BirthDate { get; set; }

        [NotExcelProperty]
        public bool SendMail { get; set; }

        [NotExcelProperty]
        public bool SendSms { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [NotExcelProperty]
        public int RegionId { get; set; }

        [NotExcelProperty]
        public string RegistrationIp { get; set; }
        public string DocumentNumber { get; set; }

        [NotExcelProperty]
        public int? DocumentType { get; set; }
        public string DocumentIssuedBy { get; set; }
        public bool IsDocumentVerified { get; set; }
        public string Address { get; set; }
        public string MobileNumber { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }

        [NotExcelProperty]
        public bool IsMobileNumberVerified { get; set; }

        [NotExcelProperty]
        public string LanguageId { get; set; }
        [NotExcelProperty]
        public byte[] TimeStamp { get; set; }
        public DateTime CreationTime { get; set; }

        [NotExcelProperty]
        public DateTime LastUpdateTime { get; set; }
        public int Group { get; set; }
        public int? State { get; set; }
        public bool? Closed { get; set; }

        [NotExcelProperty]
        public bool CallToPhone { get; set; }

        [NotExcelProperty]
        public bool SendPromotions { get; set; }

        [NotExcelProperty]
        public int? InformedFrom { get; set; }
        public string ZipCode { get; set; }
        public bool HasNote { get; set; }

        [NotExcelProperty]
        public string Info { get; set; }        
        public decimal Balance { get; set; }
        public decimal GGR { get; set; }
        public decimal NGR { get; set; }
        public int? UserId { get; set; }
        public bool AllowOutright { get; set; }
        public bool AllowParentOutright { get; set; }
        public bool AllowDoubleCommission { get; set; }
        public bool AllowParentDoubleCommission { get; set; }
        public string NickName { get; set; }
        public List<MemberCommission> Commissions1 { get; set; }
        public List<MemberCommission> Commissions2 { get; set; }
        public List<MemberCommission> Commissions3 { get; set; }
    }
}