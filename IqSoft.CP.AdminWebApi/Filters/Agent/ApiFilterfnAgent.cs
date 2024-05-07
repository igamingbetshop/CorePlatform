using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.AdminWebApi.Filters.Agent
{
    public class ApiFilterfnAgent : ApiFilterBase
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? PartnerId { get; set; }
        public int? ParentId { get; set; }
        public ApiFiltersOperation Ids { get; set; }

        public ApiFiltersOperation FirstNames { get; set; }

        public ApiFiltersOperation LastNames { get; set; }

        public ApiFiltersOperation UserNames { get; set; }

        public ApiFiltersOperation Emails { get; set; }

        public ApiFiltersOperation Genders { get; set; }

        public ApiFiltersOperation Currencies { get; set; }

        public ApiFiltersOperation LanguageIds { get; set; }
    }
}