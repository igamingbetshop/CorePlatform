using IqSoft.CP.Common.Models.WebSiteModels.Filters;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class GetTicketsInput : ApiFilterBase
    {
        public int?[] Statuses { get; set; }
    }
}