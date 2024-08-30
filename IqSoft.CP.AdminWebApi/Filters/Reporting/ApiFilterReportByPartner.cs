using IqSoft.CP.Common.Models.Filters;
using System;

namespace IqSoft.CP.AdminWebApi.Filters.Reporting
{
    public class ApiFilterReportByPartner : ApiFilterBase
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public ApiFiltersOperation PartnerIds { get; set; }
        public ApiFiltersOperation PartnerNames { get; set; }
        public ApiFiltersOperation TotalBetAmounts { get; set; }
        public ApiFiltersOperation TotalBetsCounts { get; set; }
        public ApiFiltersOperation TotalWinAmounts { get; set; }
        public ApiFiltersOperation TotalGGRs { get; set; }
    }
}