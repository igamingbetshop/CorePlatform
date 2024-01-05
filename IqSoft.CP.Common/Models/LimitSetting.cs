using System;

namespace IqSoft.CP.Common.Models
{
    public class LimitSetting
    {
        public int ClientId { get; set; }
        public int? UserId { get; set; }
        public string SettingName { get; set; }
        public decimal? SettingValue { get; set; }
        public DateTime? CreationTime { get; set; }
        public int? ApplyInDays { get; set; }
    }
}