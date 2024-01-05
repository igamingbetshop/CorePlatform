using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterRealTime : ApiFilterBase
    {
        public int? PartnerId { get; set; }

        public ApiFiltersOperation ClientIds { get; set; }

        public ApiFiltersOperation LanguageIds { get; set; }

        public ApiFiltersOperation Names { get; set; }

        public ApiFiltersOperation UserNames { get; set; }

        public ApiFiltersOperation Categories { get; set; }

        public ApiFiltersOperation RegionIds { get; set; }

        public ApiFiltersOperation Currencies { get; set; }

        public ApiFiltersOperation LoginIps { get; set; }

        public ApiFiltersOperation Balances { get; set; }

        public ApiFiltersOperation TotalDepositsCounts { get; set; }

        public ApiFiltersOperation TotalDepositsAmounts { get; set; }

        public ApiFiltersOperation TotalWithdrawalsCounts { get; set; }

        public ApiFiltersOperation TotalWithdrawalsAmounts { get; set; }

        public ApiFiltersOperation TotalBetsCounts { get; set; }

        public ApiFiltersOperation GGRs { get; set; }
    }
}