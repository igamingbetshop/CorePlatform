using IqSoft.CP.Common.Models.Filters;
using System;

namespace IqSoft.CP.AdminWebApi.Filters.Clients
{
    public class ApiFilterfnSegmentClient : ApiFilterBase
    {
        public int? PartnerId { get; set; }
        public int? SegmentId { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public ApiFiltersOperation Ids { get; set; }
        public ApiFiltersOperation Emails { get; set; }
        public ApiFiltersOperation UserNames { get; set; }
        public ApiFiltersOperation Currencies { get; set; }
        public ApiFiltersOperation LanguageIds { get; set; }
        public ApiFiltersOperation Categories { get; set; }
        public ApiFiltersOperation Genders { get; set; }
        public ApiFiltersOperation FirstNames { get; set; }
        public ApiFiltersOperation LastNames { get; set; }
        public ApiFiltersOperation SecondNames { get; set; }
        public ApiFiltersOperation SecondSurnames { get; set; }
        public ApiFiltersOperation DocumentNumbers { get; set; }
        public ApiFiltersOperation DocumentIssuedBys { get; set; }
        public ApiFiltersOperation MobileNumbers { get; set; }
        public ApiFiltersOperation ZipCodes { get; set; }
        public ApiFiltersOperation IsDocumentVerifieds { get; set; }
        public ApiFiltersOperation PhoneNumbers { get; set; }
        public ApiFiltersOperation RegionIds { get; set; }
        public ApiFiltersOperation BirthDates { get; set; }               
        public ApiFiltersOperation States { get; set; }               
        public ApiFiltersOperation CreationTimes { get; set; }
        public ApiFiltersOperation SegmentIds { get; set; }               
        public ApiFiltersOperation UserIds { get; set; }
        public ApiFiltersOperation AffiliatePlatformIds { get; set; }
        public ApiFiltersOperation AffiliateIds { get; set; }
        public ApiFiltersOperation AffiliateReferralIds { get; set; }
    }
}