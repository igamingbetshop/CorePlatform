﻿using System;

namespace IqSoft.CP.AdminWebApi.Filters.Reporting
{
    public class ApiFilterReportByClientSession : ApiFilterBase
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public ApiFiltersOperation Ids { get; set; }
        public ApiFiltersOperation PartnerIds { get; set; }
        public ApiFiltersOperation ClientIds { get; set; }
        public ApiFiltersOperation FirstNames { get; set; }
        public ApiFiltersOperation LastNames { get; set; }
        public ApiFiltersOperation UserNames { get; set; }
        public ApiFiltersOperation LanguageIds { get; set; }
        public ApiFiltersOperation Ips { get; set; }
        public ApiFiltersOperation Countries { get; set; }
        public ApiFiltersOperation DeviceTypes { get; set; }
        public ApiFiltersOperation States { get; set; }
        public ApiFiltersOperation LogoutTypes { get; set; }

    }
}