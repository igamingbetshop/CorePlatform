using System;

namespace IqSoft.CP.AdminWebApi.Filters.PaymentRequests
{
    public class ApiFilterfnPaymentRequest : ApiFilterBase
    {
        public int? PartnerId { get; set; }
        public long? AccountId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? Type { get; set; }
        public bool? HasNote { get; set; }
        public int? AgentId { get; set; }

        public ApiFiltersOperation Ids { get; set; }
        public ApiFiltersOperation UserNames { get; set; }
        public ApiFiltersOperation Names { get; set; }
        public ApiFiltersOperation CreatorNames { get; set; }
        public ApiFiltersOperation ClientIds { get; set; }
        public ApiFiltersOperation Emails { get; set; }
        public ApiFiltersOperation UserIds { get; set; }
        public ApiFiltersOperation PartnerPaymentSettingIds { get; set; }
        public ApiFiltersOperation PaymentSystemIds { get; set; }
        public ApiFiltersOperation CurrencyIds { get; set; }
        public ApiFiltersOperation States { get; set; }
        public ApiFiltersOperation Types { get; set; }
        public ApiFiltersOperation BetShopIds { get; set; }
        public ApiFiltersOperation BetShopNames{ get; set; }
        public ApiFiltersOperation Amounts { get; set; }
        public ApiFiltersOperation CreationTimes { get; set; }
        public ApiFiltersOperation LastUpdateTimes { get; set; }
		public ApiFiltersOperation ExternalIds { get; set; }
		public ApiFiltersOperation AffiliatePlatformIds { get; set; }
		public ApiFiltersOperation AffiliateIds { get; set; }
        public ApiFiltersOperation ActivatedBonusTypes { get; set; }
        public ApiFiltersOperation CommissionAmounts { get; set; }
        public ApiFiltersOperation CardNumbers { get; set; }
        public ApiFiltersOperation CountryCodes { get; set; }
        public ApiFiltersOperation SegmentIds { get; set; }
        public ApiFiltersOperation SegmentNames { get; set; }
    }
}