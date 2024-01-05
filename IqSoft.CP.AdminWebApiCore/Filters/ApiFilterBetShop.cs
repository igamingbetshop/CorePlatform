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
    }
}
