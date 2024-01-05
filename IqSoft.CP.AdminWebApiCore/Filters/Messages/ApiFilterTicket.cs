using System;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Filters.Messages
{
    public class ApiFilterTicket : ApiFilterBase
    {
        public ApiFiltersOperation Ids { get; set; }

        public ApiFiltersOperation ClientIds { get; set; }

        public ApiFiltersOperation UserNames { get; set; }

        public ApiFiltersOperation PartnerIds { get; set; }

        public ApiFiltersOperation PartnerNames { get; set; }

        public ApiFiltersOperation Subjects { get; set; }

        public ApiFiltersOperation UserIds { get; set; }
        
        public ApiFiltersOperation UserFirstNames { get; set; }
        
        public ApiFiltersOperation UserLastNames { get; set; }

        public ApiFiltersOperation Statuses { get; set; }

        public List<int> Types { get; set; }

        public int? State { get; set; }
        
        public bool UnreadsOnly { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }
    }
}