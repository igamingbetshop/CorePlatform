using System;

namespace IqSoft.CP.AdminWebApi.Filters.Reporting
{
    public class ApiFilterReportByUserSession : ApiFilterBase
    {
        public int? PartnerId { get; set; }
        public int? UserId { get; set; }
        public int? Type { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public ApiFiltersOperation Ids { get; set; }
        public ApiFiltersOperation PartnerIds { get; set; }
        public ApiFiltersOperation UserIds { get; set; }
        public ApiFiltersOperation UserNames { get; set; }
        public ApiFiltersOperation FirstNames { get; set; }
        public ApiFiltersOperation LastNames { get; set; }
        public ApiFiltersOperation Emails { get; set; }
        public ApiFiltersOperation LanguageIds { get; set; }
        public ApiFiltersOperation Ips { get; set; }
        public ApiFiltersOperation States { get; set; }
        public ApiFiltersOperation Types { get; set; }
        public ApiFiltersOperation LogoutTypes { get; set; }
        public ApiFiltersOperation EndTimes { get; set; }
    }
}