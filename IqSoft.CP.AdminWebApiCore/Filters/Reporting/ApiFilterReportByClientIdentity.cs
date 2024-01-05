using System;
namespace IqSoft.CP.AdminWebApi.Filters.Reporting
{
    public class ApiFilterReportByClientIdentity : ApiFilterBase
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool? HasNote { get; set; }
        public ApiFiltersOperation Ids { get; set; }
        public ApiFiltersOperation ClientIds { get; set; }
        public ApiFiltersOperation PartnerIds { get; set; }
        public ApiFiltersOperation UserIds { get; set; }
        public ApiFiltersOperation DocumentTypeIds { get; set; }
        public ApiFiltersOperation States { get; set; }
        public ApiFiltersOperation ExpirationTimes { get; set; }
    }
}