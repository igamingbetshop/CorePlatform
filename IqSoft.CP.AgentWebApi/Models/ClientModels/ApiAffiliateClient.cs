using System;

namespace IqSoft.CP.AgentWebApi.Models.ClientModels
{
    public class ApiAffiliateClient
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string UserName { get; set; }
        public string CurrencyId { get; set; }
        public DateTime CreationDate { get; set; }
        public string RefId { get; set; }
        public string AffiliateId { get; set; }
        public int AffiliatePlaformId { get; set; }
        public decimal? TotalDepositAmount { get; set; }
        public decimal? ConvertedTotalDepositAmount { get; set; }
        public DateTime? FirstDepositDate { get; set; }
        public DateTime? LastDepositDate { get; set; }

    }
}