using System;

namespace IqSoft.CP.AgentWebApi.Filters
{
    public class ApiFilterClientCorrection: ApiFilterBase
    {
        public int? ClientId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }


        public ApiFiltersOperation Ids { get; set; }
        public ApiFiltersOperation ClientIds { get; set; }
        public ApiFiltersOperation Amounts { get; set; }
        public ApiFiltersOperation CurrencyIds { get; set; }
        public ApiFiltersOperation States { get; set; }
        public ApiFiltersOperation UserIds { get; set; }
        public ApiFiltersOperation OperationTypeNames { get; set; }
        public ApiFiltersOperation FirstNames { get; set; }
        public ApiFiltersOperation LastNames { get; set; }
    }
}