using System;

namespace IqSoft.CP.AdminWebApi.Filters.Reporting
{
    public class ApiFilterReportByActionLog : ApiFilterBase
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string UserIdentity { get; set; }
        public ApiFiltersOperation Ids { get; set; }
        public ApiFiltersOperation ActionNames { get; set; }
        public ApiFiltersOperation ActionGroups { get; set; }
        public ApiFiltersOperation UserIds { get; set; }
        public ApiFiltersOperation Domains { get; set; }
        public ApiFiltersOperation Sources { get; set; }
        public ApiFiltersOperation Ips { get; set; }
        public ApiFiltersOperation Countries { get; set; }
        public ApiFiltersOperation SessionIds { get; set; }
        public ApiFiltersOperation Pages { get; set; }
        public ApiFiltersOperation Languages { get; set; }
        public ApiFiltersOperation ResultCodes { get; set; }
        public ApiFiltersOperation Descriptions { get; set; }
    }
}