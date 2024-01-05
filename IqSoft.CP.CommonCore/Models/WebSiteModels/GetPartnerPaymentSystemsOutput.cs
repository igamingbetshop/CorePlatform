using System;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class GetPartnerPaymentSystemsOutput : ApiResponseBase
    {
        public List<PartnerPaymentSettingModel> PartnerPaymentSystems { get; set; }
    }

    public class PartnerPaymentSettingModel
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public int PaymentSystemId { get; set; }
        public decimal Commission { get; set; }
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
        public string Info { get; set; }
        public string Address { get; set; }
        public string DestinationTag { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal? CommissionPercent { get; set; }
    }
}