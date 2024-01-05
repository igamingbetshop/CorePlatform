using System;

namespace IqSoft.CP.AgentWebApi.Filters
{
    public class ApiFilterfnClient : ApiFilterBase
    {
        public int? PartnerId { get; set; }
        public string AgentIdentifier { get; set; }
        public int? AgentId { get; set; }
        public int? ClientId { get; set; }
        public bool? AllowDoubleCommission { get; set; }
        public bool? WithDownlines { get; set; }
        public int? State { get; set; }
        public int? AffiliateReferralId { get; set; }
        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }

        public ApiFiltersOperation Ids { get; set; }

        public ApiFiltersOperation Emails { get; set; }

        public ApiFiltersOperation UserNames { get; set; }

        public ApiFiltersOperation CurrencyIds { get; set; }

        public ApiFiltersOperation LanguageIds { get; set; }

        public ApiFiltersOperation Categories { get; set; }

        public ApiFiltersOperation Genders { get; set; }

        public ApiFiltersOperation FirstNames { get; set; }

        public ApiFiltersOperation LastNames { get; set; }

        public ApiFiltersOperation DocumentNumbers { get; set; }

        public ApiFiltersOperation DocumentIssuedBys { get; set; }

        public ApiFiltersOperation Addresses { get; set; }

        public ApiFiltersOperation MobileNumbers { get; set; }

        public ApiFiltersOperation ZipCodes { get; set; }

        public ApiFiltersOperation IsDocumentVerifieds { get; set; }

        public ApiFiltersOperation PhoneNumbers { get; set; }

        public ApiFiltersOperation RegionIds { get; set; }

        public ApiFiltersOperation BirthDates { get; set; }

        public ApiFiltersOperation States { get; set; }

        public ApiFiltersOperation CreationTimes { get; set; }

        public ApiFiltersOperation Balances { get; set; }

        public ApiFiltersOperation GGRs { get; set; }

        public ApiFiltersOperation NETGamings { get; set; }

        public ApiFiltersOperation AffiliatePlatformIds { get; set; }

        public ApiFiltersOperation AffiliateIds { get; set; }

        public ApiFiltersOperation UserIds { get; set; }
        public ApiFiltersOperation RefIds { get; set; }
        public ApiFiltersOperation AffiliateReferralIds { get; set; }
        public ApiFiltersOperation ReferralIds { get; set; }
        public ApiFiltersOperation FirstDepositDates { get; set; }
        public ApiFiltersOperation LastDepositDates { get; set; }
        public ApiFiltersOperation TotalDepositAmounts { get; set; }
        public ApiFiltersOperation ConvertedTotalDepositAmounts { get; set; }
    }
}