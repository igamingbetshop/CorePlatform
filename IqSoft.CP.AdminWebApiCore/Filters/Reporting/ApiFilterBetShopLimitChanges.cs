using System;

namespace IqSoft.CP.AdminWebApi.Filters.Reporting
{
    public class ApiFilterBetShopLimitChanges : ApiFilterBase
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public ApiFiltersOperation BetShopIds { get; set; }

        public ApiFiltersOperation UserIds { get; set; } 
    }
}