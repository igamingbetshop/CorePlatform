using System;
using IqSoft.CP.Common.Attributes;
using Newtonsoft.Json;

namespace IqSoft.CP.AdminWebApi.Models.PaymentModels
{
    public class ApiPaymentRequest
    {
        public long Id { get; set; }
        [NotExcelProperty]
        public int PartnerId { get; set; }
        public int? ClientId { get; set; }

        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "Email")]
        public string ClientEmail { get; set; }
        [NotExcelProperty]
        public string ClientDocumentNumber { get; set; }
        public string CurrencyId { get; set; }
        public decimal Amount { get; set; }
        [NotExcelProperty]
        public int State { get; set; }

        [JsonProperty(PropertyName = "State"), JsonIgnore]
        public string PaymentStatus { get; set; }

        [NotExcelProperty]
        public int Type { get; set; }
        public string ExternalId { get; set; }
        public string CardNumber { get; set; }
        public string BankName { get; set; }
        public string CountryCode { get; set; }
        public string TransactionIp { get; set; }
        public string CardType { get; set; }

        [NotExcelProperty]
        public int PaymentSystemId { get; set; }

        [JsonProperty(PropertyName = "PaymentSystemId"), JsonIgnore]
        public string PaymentSystemName { get; set; }

        [NotExcelProperty]
        public int? PartnerPaymentSettingId { get; set; }
        [NotExcelProperty]
        public int? BetShopId { get; set; }
        [NotExcelProperty]
        public long? Barcode { get; set; }
        [NotExcelProperty]
        public string BetShopName { get; set; }
        [NotExcelProperty]
        public string BetShopAddress { get; set; }
        [NotExcelProperty]
        public int? CashDeskId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        [NotExcelProperty]
        public bool ClientHasNote { get; set; }
        public int GroupId { get; set; }
        [NotExcelProperty]
        public int? CreatorId { get; set; }
        [NotExcelProperty]
        public string CreatorFirstName { get; set; }
        [NotExcelProperty]
        public string CreatorLastName { get; set; }
        [NotExcelProperty]
        public bool HasNote { get; set; }
        [NotExcelProperty]
        public string CashCode { get; set; }
        [NotExcelProperty]
        public string Parameters { get; set; }
        public int? AffiliatePlatformId { get; set; }
        public string AffiliateId { get; set; }
        public int? ActivatedBonusType { get; set; }
        public decimal? CommissionPercent { get; set; }
        public decimal? CommissionAmount { get; set; }
        public decimal FinalAmount { get; set; }
        [NotExcelProperty]
        public string PaymentForm { get; set; }
        public string SegmentName { get; set; }
        public int? SegmentId { get; set; }

        [NotExcelProperty]
        public string Info { get; set; }
        public int DepositCount { get; set; }

        [NotExcelProperty]
        public long? ParentId { get; set; }
    }
}