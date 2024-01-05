using System;
namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterAnnouncement : ApiFilterBase
    {
        public int? PartnerId { get; set; }
        public int? ReceiverId { get; set; }
        public int? Type { get; set; }
        public int? ReceiverTypeId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public ApiFiltersOperation Ids { get; set; }
        public ApiFiltersOperation PartnerIds { get; set; }
        public ApiFiltersOperation UserIds { get; set; }
        public ApiFiltersOperation ReceiverIds { get; set; }
        public ApiFiltersOperation Types { get; set; }
        public ApiFiltersOperation States { get; set; }
    }
}