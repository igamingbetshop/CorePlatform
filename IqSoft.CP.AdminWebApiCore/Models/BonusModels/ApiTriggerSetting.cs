using IqSoft.CP.AdminWebApi.Models.CommonModels;
using IqSoft.CP.Common.Models.Bonus;
using IqSoft.CP.Common.Models.AdminModels;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.BonusModels
{
    public class ApiTriggerSetting
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long? TranslationId { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime FinishTime { get; set; }
        public int? Percent { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public bool? Activate { get; set; }
        public int? MinBetCount { get; set; }
        public string BonusSettingCodes { get; set; }
        public int PartnerId { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime UpdateTime { get; set; }
        public int Order { get; set; }
        public string FileData { get; set; }
        public int Status { get; set; }
        public string PromoCode { get; set; }
        public int ClientId { get; set; }
        public decimal? SourceAmount { get; set; }
        public List<ApiBonusProducts> Products { get; set; }
        public ApiSetting PaymentSystemIds { get; set; }
        public string Sequence { get; set; }
        public int? BetCount { get; set; }
        public int? SegmentId { get; set; }
        public int? DayOfWeek { get; set; }
        public decimal? WageringAmount { get; set; }
        public decimal? UpToAmount { get; set; }
        public BonusCondition Conditions { get; set; }
    }
}