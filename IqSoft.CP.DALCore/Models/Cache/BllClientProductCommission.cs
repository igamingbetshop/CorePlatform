using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllClientProductCommission
    {
        public decimal? Percent { get; set; }
        public string TurnoverPercent { get; set; }
        public int? ClientId { get; set; }
        public int? AgentId { get; set; }
    }
}
