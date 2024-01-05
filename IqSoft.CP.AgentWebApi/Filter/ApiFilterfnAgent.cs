using System;

namespace IqSoft.CP.AgentWebApi.Filters
{
    public class ApiFilterfnAgent : ApiFilterBase
    {
        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }

        public ApiFiltersOperation Ids { get; set; }
               
        public ApiFiltersOperation FirstNames { get; set; }
               
        public ApiFiltersOperation LastNames { get; set; }
               
        public ApiFiltersOperation UserNames { get; set; }
               
        public ApiFiltersOperation Emails { get; set; }
               
        public ApiFiltersOperation Genders { get; set; }
               
        public ApiFiltersOperation UserStates { get; set; }
               
        public ApiFiltersOperation ClientCounts { get; set; }

        public ApiFiltersOperation Balances { get; set; }
    }
}