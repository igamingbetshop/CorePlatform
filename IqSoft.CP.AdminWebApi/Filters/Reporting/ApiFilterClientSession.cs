using System;
using IqSoft.CP.Common.Models.Filters;

namespace IqSoft.CP.AdminWebApi.Filters.Reporting
{
    public class ApiFilterClientSession : ApiFilterBase
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int ClientId { get; set; }
        public ApiFiltersOperation Ips { get; set; }
        public ApiFiltersOperation Countries { get; set; }
        public ApiFiltersOperation ProductIds { get; set; }
        public ApiFiltersOperation DeviceTypes { get; set; }
        public ApiFiltersOperation States { get; set; }
        public ApiFiltersOperation Sources { get; set; }
        public ApiFiltersOperation LanguageIds { get; set; }
        public ApiFiltersOperation LogoutTypes { get; set; }
    }
}