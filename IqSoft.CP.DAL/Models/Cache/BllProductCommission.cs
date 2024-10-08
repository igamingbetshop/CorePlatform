using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllProductCommission
    {
        public decimal? GGRSharePercent { get; set; }
        public string TurnoverSharePercent { get; set; }
        public int? ClientId { get; set; }
        public int? AgentId { get; set; }
    }
}
