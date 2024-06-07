using System;

namespace IqSoft.CP.AdminWebApi.Filters.Affiliate
{
    public class ApiFilterAffiliateCorrection : ApiFilterBase
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? AffiliateId { get; set; }
        public ApiFiltersOperation Ids { get; set; }
        public ApiFiltersOperation PartnerIds { get; set; }
        public ApiFiltersOperation AffiliateIds { get; set; }
        public ApiFiltersOperation FirstNames { get; set; }
        public ApiFiltersOperation LastNames { get; set; }
        public ApiFiltersOperation Amounts { get; set; }
        public ApiFiltersOperation CurrencyIds { get; set; }
        public ApiFiltersOperation Creators { get; set; }
        public ApiFiltersOperation CreationTimes { get; set; }
        public ApiFiltersOperation LastUpdateTimes { get; set; }
        public ApiFiltersOperation OperationTypeNames { get; set; }
        public ApiFiltersOperation CreatorFirstNames { get; set; }
        public ApiFiltersOperation CreatorLastNames { get; set; }
        public ApiFiltersOperation DocumentTypeIds { get; set; }
        public ApiFiltersOperation ClientIds { get; set; }
        public ApiFiltersOperation ClientFirstNames { get; set; }
        public ApiFiltersOperation ClientLastNames { get; set; }
    }
}