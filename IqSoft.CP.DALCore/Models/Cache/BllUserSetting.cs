using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllUserSetting
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? ParentState { get; set; }
        public bool? AllowAutoPT { get; set; }
        public bool AllowOutright { get; set; }
        public bool AllowDoubleCommission { get; set; }
        public string CalculationPeriod { get; set; }
        public bool? IsCalculationPeriodBlocked { get; set; }
        public decimal? AgentMaxCredit { get; set; }
        public int? OddsType { get; set; }
        public string LevelLimits { get; set; }
        public string CountLimits { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
}
