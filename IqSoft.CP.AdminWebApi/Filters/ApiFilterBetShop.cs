using IqSoft.CP.Common.Models.Filters;
using System;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterBetShop : ApiFilterBase
    {
        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }

        public int? PartnerId { get; set; }

        public ApiFiltersOperation Ids { get; set; }
        public ApiFiltersOperation GroupIds { get; set; }
        public ApiFiltersOperation States { get; set; }
        public ApiFiltersOperation CurrencyIds { get; set; }
        public ApiFiltersOperation Names { get; set; }
        public ApiFiltersOperation Addresses { get; set; }
        public ApiFiltersOperation Balances { get; set; }
        public ApiFiltersOperation CurrentLimits { get; set; }
        public ApiFiltersOperation AgentIds { get; set; }
        public ApiFiltersOperation MaxCopyCounts { get; set; }
        public ApiFiltersOperation MaxWinAmounts { get; set; }
        public ApiFiltersOperation MinBetAmounts { get; set; }
        public ApiFiltersOperation MaxEventCountPerTickets { get; set; }
        public ApiFiltersOperation CommissionTypes { get; set; }
        public ApiFiltersOperation CommissionRates { get; set; }
        public ApiFiltersOperation AnonymousBets { get; set; }
        public ApiFiltersOperation AllowCashouts { get; set; }
        public ApiFiltersOperation AllowLives { get; set; }
        public ApiFiltersOperation UsePins { get; set; }
    }
}
