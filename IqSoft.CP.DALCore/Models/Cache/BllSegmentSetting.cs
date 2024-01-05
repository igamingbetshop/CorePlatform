using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllSegmentSetting
    {
        public int Id { get; set; }
        public int SegmentId { get; set; }
        public string Name { get; set; }
        public string StringValue { get; set; }
        public decimal? NumericValue { get; set; }
        public DateTime? DateValue { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
}