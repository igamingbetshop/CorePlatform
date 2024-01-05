using IqSoft.CP.AgentWebApi.Filters;

namespace IqSoft.CP.AgentWebApi.Models.Affiliate
{
    public class ApiFilterfnAffiliateClientInfo : ApiFilterBase
    {
        public int? PartnerId { get; set; }
        public ApiFiltersOperation Ids { get; set; }
        public ApiFiltersOperation UserNames { get; set; }
        public ApiFiltersOperation CurrencyIds { get; set; }
        public ApiFiltersOperation CreationDates { get; set; }
        public ApiFiltersOperation RefIds { get; set; }
        public ApiFiltersOperation AffiliateIds { get; set; }
        public ApiFiltersOperation AffiliateReferralIds { get; set; }
        public ApiFiltersOperation ReferralIds { get; set; }
        public ApiFiltersOperation FirstDepositDates { get; set; }
        public ApiFiltersOperation LastDepositDates { get; set; }
        public ApiFiltersOperation TotalDepositAmounts { get; set; }
        public ApiFiltersOperation ConvertedTotalDepositAmounts { get; set; }
    }
}