using System;

namespace IqSoft.CP.AdminWebApi.Filters.Reporting
{
    public class ApiFilterReportByAgentTranfer : ApiFilterBase
    {
        public int? PartnerId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public ApiFiltersOperation PartnerIds { get; set; }
        public ApiFiltersOperation UserIds { get; set; }
        public ApiFiltersOperation UserNames { get; set; }
        public ApiFiltersOperation NickNames { get; set; }
        public ApiFiltersOperation TotoalProfits { get; set; }
        public ApiFiltersOperation TotalDebits { get; set; }
        public ApiFiltersOperation Balances { get; set; }

    }
}