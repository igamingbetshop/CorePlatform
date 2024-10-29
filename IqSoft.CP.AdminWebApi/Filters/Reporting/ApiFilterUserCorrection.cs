using IqSoft.CP.Common.Models.Filters;
using System;

namespace IqSoft.CP.AdminWebApi.Filters.Reporting
{
    public class ApiFilterUserCorrection : ApiFilterBase
    {
        public int? PartnerId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public ApiFiltersOperation PartnerIds { get; set; }
        public ApiFiltersOperation UserIds { get; set; }
        public ApiFiltersOperation UserNames { get; set; }
        public ApiFiltersOperation TotalDebits { get; set; }
        public ApiFiltersOperation TotalCredits { get; set; }
        public ApiFiltersOperation Balances { get; set; }
    }
}