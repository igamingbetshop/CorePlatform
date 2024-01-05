using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllClientClassification
    {
        public int Id { get; set; }

        public int ClientId { get; set; }

        public int? CategoryId { get; set; }

        public int ProductId { get; set; }

        public long SessionId { get; set; }
        public int? SegmentId { get; set; }

        public System.DateTime LastUpdateTime { get; set; }

        public int? State { get; set; }
    }
}
