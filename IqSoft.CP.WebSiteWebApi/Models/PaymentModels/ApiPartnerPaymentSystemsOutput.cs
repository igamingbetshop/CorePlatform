using IqSoft.CP.Common.Models.WebSiteModels;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.WebSiteWebApi.Models.PaymentModels
{
    public class ApiPartnerPaymentSystemsOutput : ApiResponseBase
    {
        public List<ApiPartnerPaymentSystem> PartnerPaymentSystems { get; set; }
    }

    public class ApiPartnerPaymentSystem
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public int PaymentSystemId { get; set; }
        public decimal CommissionPercent { get; set; }
        public decimal FixedFee { get; set; }
        public int State { get; set; }
        public int PaymentSystemType { get; set; }
        public string CurrencyId { get; set; }
        public DateTime CreationTime { get; set; }
        public string PaymentSystemName { get; set; }
        public int PaymentSystemPriority { get; set; }
		public bool HasBank { get; set; }

		public int Type { get; set; }
        public int ContentType { get; set; }
        public List<decimal> Info { get; set; }
        public string Address { get; set; }
        public string ImageExtension { get; set; }
        public string DestinationTag { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
    }
}