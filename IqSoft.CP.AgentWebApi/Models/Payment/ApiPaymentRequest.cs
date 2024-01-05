using System;
using IqSoft.CP.Common.Attributes;

namespace IqSoft.CP.AgentWebApi.Models.Payment
{
    public class ApiPaymentRequest
    {
        public long Id { get; set; }

        [NotExcelProperty]
        public int PartnerId { get; set; }

        public int ClientId { get; set; }

        public decimal Amount { get; set; }

        public string CurrencyId { get; set; }

        public int Status { get; set; }
        public int? PartnerPaymentSettingId { get; set; }
        public int? BetShopId { get; set; }
        public long? Barcode { get; set; }

        public string BetShopName { get; set; }
        public string BetShopAddress { get; set; }
        public int Type { get; set; }

        public int PaymentSystemId { get; set; }

        public string PaymentSystemName { get; set; }

        public string Info { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public Nullable<int> CashDeskId { get; set; }

        public string UserName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ClientDocumentNumber { get; set; }

        public bool ClientHasNote { get; set; }

        public int GroupId { get; set; }

        public int? CreatorId { get; set; }
            
        public string CreatorFirstName { get; set; }

        public string CreatorLastName { get; set; }

        public bool HasNote { get; set; }

        public string CashCode { get; set; }

		public string ExternalId { get; set; }

		public string Parameters { get; set; }

		public int? AffiliatePlatformId { get; set; }

		public string AffiliateId { get; set; }

        public int? ActivatedBonusType { get; set; }
        public string PaymentForm { get; set; }
    }
}