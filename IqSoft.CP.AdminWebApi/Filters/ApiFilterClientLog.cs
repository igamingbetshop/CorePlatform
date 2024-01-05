using System;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterClientLog : ApiFilterBase
    {
        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }

        public ApiFiltersOperation Ids { get; set; }

        public ApiFiltersOperation ClientIds { get; set; }

        public ApiFiltersOperation Actions { get; set; }

        public ApiFiltersOperation UserIds { get; set; }

        public ApiFiltersOperation Ips { get; set; }

        public ApiFiltersOperation Pages { get; set; }

        public ApiFiltersOperation SessionIds { get; set; }
    }
}