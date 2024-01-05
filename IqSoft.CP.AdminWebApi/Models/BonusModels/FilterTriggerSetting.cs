using IqSoft.CP.AdminWebApi.Filters;

namespace IqSoft.CP.AdminWebApi.Models.BonusModels
{
    public class FilterTriggerSetting : ApiFilterBase
    {
        public int? Id { get; set; }
        public int? PartnerId { get; set; }
        public int? BonusId { get; set; }
        public int? Status { get; set; }
    }
}