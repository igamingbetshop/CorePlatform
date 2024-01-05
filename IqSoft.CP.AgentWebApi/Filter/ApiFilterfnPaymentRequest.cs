using System;

namespace IqSoft.CP.AgentWebApi.Filters
{
    public class ApiFilterfnPaymentRequest : ApiFilterBase
    {
        public int? PartnerId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public int? Type { get; set; }

        public int? AgentId { get; set; }

        public ApiFiltersOperation Ids { get; set; }

        public ApiFiltersOperation UserNames { get; set; }

        public ApiFiltersOperation Names { get; set; }

        public ApiFiltersOperation CreatorNames { get; set; }

        public ApiFiltersOperation ClientIds { get; set; }

        public ApiFiltersOperation UserIds { get; set; }

        public ApiFiltersOperation PartnerPaymentSettingIds { get; set; }

        public ApiFiltersOperation PaymentSystemIds { get; set; }

        public ApiFiltersOperation Currencies { get; set; }

        public ApiFiltersOperation States { get; set; }

        public ApiFiltersOperation BetShopIds { get; set; }

        public ApiFiltersOperation BetShopNames{ get; set; }

        public ApiFiltersOperation Amounts { get; set; }

        public ApiFiltersOperation CreationDates { get; set; }

        public ApiFiltersOperation LastUpdateDates { get; set; }

		public ApiFiltersOperation ExternalIds { get; set; }

		public ApiFiltersOperation AffiliatePlatformIds { get; set; }

		public ApiFiltersOperation AffiliateIds { get; set; }

        public ApiFiltersOperation ActivatedBonusTypes { get; set; }

    }
}