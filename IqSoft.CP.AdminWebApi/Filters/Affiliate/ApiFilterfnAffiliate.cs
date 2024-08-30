using IqSoft.CP.Common.Models.Filters;
using System;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterfnAffiliate : ApiFilterBase
    {
        public int? PartnerId { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }

        public ApiFiltersOperation Ids { get; set; }

        public ApiFiltersOperation Emails { get; set; }

        public ApiFiltersOperation UserNames { get; set; }

        public ApiFiltersOperation FirstNames { get; set; }

        public ApiFiltersOperation LastNames { get; set; }
        public ApiFiltersOperation MobileNumbers { get; set; }

        public ApiFiltersOperation RegionIds { get; set; }
        public ApiFiltersOperation States { get; set; }

        public ApiFiltersOperation CreationTimes { get; set; }
    }
}