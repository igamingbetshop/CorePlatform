using System;
namespace IqSoft.CP.AdminWebApi.Filters.Clients
{
    public class ApiFilterfnDuplicateClient : ApiFilterBase
    {
        public int? PartnerId { get; set; }
        public int? ClientId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public ApiFiltersOperation PartnerIds { get; set; }
        public ApiFiltersOperation ClientIds { get; set; }
        public ApiFiltersOperation MatchedClientIds { get; set; }
        public ApiFiltersOperation MatchedDatas { get; set; }
        public ApiFiltersOperation MatchDates { get; set; }
    }
}