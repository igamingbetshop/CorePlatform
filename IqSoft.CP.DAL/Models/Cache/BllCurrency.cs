using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllCurrency
    {
        public string Id { get; set; }

        public decimal CurrentRate { get; set; }

        public string Symbol { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public int Type { get; set; }
    }
}
