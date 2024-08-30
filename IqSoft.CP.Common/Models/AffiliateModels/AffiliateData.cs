using System;

namespace IqSoft.CP.Common.Models.AffiliateModels
{
    public class AffiliateData
    {
        public int PartnerId { get; set; }
        public int? AffiliatePlatformId { get; set; }
        public string AffiliatePlatformName { get; set; }
        public string AffiliatePlatId { get; set; }
        public string ClickId { get; set; }
        public int ClientId { get; set; }
        public string CurrencyId { get; set; }
        public decimal? Amount { get; set; }
        public int? DepositCount { get; set; }
        public string TransactionId { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
