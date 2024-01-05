using System;

namespace IqSoft.CP.DAL.Models
{
    public class ProductLimit
    {
        public long Id { get; set; }

        public int ObjectTypeId { get; set; }
        
        public long ObjectId { get; set; }

        public int? ProductId { get; set; }

        public decimal? MaxLimit { get; set; }

        public decimal? MinLimit { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }
}
