using System;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterClientMessage : ApiFilterBase
    {
        public ApiFiltersOperation Ids { get; set; }

        public ApiFiltersOperation ClientIds { get; set; }

        public ApiFiltersOperation UserNames { get; set; }

        public ApiFiltersOperation Subjects { get; set; }

        public ApiFiltersOperation PartnerIds { get; set; }

        public ApiFiltersOperation Statuses { get; set; }
        public ApiFiltersOperation MobileOrEmails { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }
    }
}