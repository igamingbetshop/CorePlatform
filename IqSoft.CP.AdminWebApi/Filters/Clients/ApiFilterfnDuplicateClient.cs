using IqSoft.CP.Common.Models.Filters;
namespace IqSoft.CP.AdminWebApi.Filters.Clients
{
    public class ApiFilterfnDuplicateClient : ApiFilterBase
    {
        public int ClientId { get; set; }
        public ApiFiltersOperation DuplicatedClientIds { get; set; }
        public ApiFiltersOperation DuplicatedDatas { get; set; }
        public ApiFiltersOperation MatchDates { get; set; }
    }
}