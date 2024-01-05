using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllProductLimit
    {
        public long Id { get; set; }

        public long ObjectId { get; set; }

        public int ObjectTypeId { get; set; }

        public int ProductId { get; set; }

        public int LimitTypeId { get; set; }

        public decimal? MinLimit { get; set; }

        public decimal? MaxLimit { get; set; }
    }
}
