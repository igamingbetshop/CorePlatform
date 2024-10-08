using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class TriggerSettingInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long TranslationId { get; set; }
        public int Type { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime FinishTime { get; set; }
        public int? Percent { get; set; }
        public string BonusSettingCodes { get; set; }
        public int PartnerId { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public decimal? Amount { get; set; }
        public int? MinBetCount { get; set; }
        public string Condition { get; set; }
        public int? SegmentId { get; set; }
        public int? DayOfWeek { get; set; }
        public decimal? UpToAmount { get; set; }
        public bool? ConsiderBonusBets { get; set; }
        public List<int> PaymentSystemIds { get; set; }
        public List<TriggerProductInfo> ProductSettings { get; set; }
    }
}
