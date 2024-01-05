using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllAsianCommissionPlan
    {
        public List<BllCommissionGroup> Groups { get; set; }
        public List<BllBetSetting> BetSettings { get; set; }
        public List<BllPositionTaking> PositionTaking { get; set; }
    }

    [Serializable]
    public class BllCommissionGroup
    {
        public int Id { get; set; }
        public decimal Value { get; set; }
        public decimal? ParentValue { get; set; }
    }

    [Serializable]
    public class BllBetSetting
    {
        public int Id { get; set; }
        public bool PreventBetting { get; set; }
        public bool ParentPreventBetting { get; set; }
        public decimal? MinBet { get; set; }
        public decimal MinBetLimit { get; set; }
        public decimal? MaxBet { get; set; }
        public decimal MaxBetLimit { get; set; }
        public decimal? MaxPerMatch { get; set; }
        public decimal MaxPerMatchLimit { get; set; }
        public string Name { get; set; }
        public int IsParlay { get; set; }
    }

    [Serializable]
    public class BllPositionTaking
    {
        public int SportId { get; set; }
        public List<BllMarketType> MarketTypes { get; set; }
    }

    [Serializable]
    public class BllMarketType
    {
        public int Id { get; set; }
        public decimal AgentPercent { get; set; }
        public bool AutoPT { get; set; }
        public decimal OwnerPercent { get; set; }
        public string Name { get; set; }
        public int? IsLive { get; set; }
    }
}
