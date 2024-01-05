using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.AgentModels
{
    public class AsianCommissionPlan
    {
        public List<CommissionGroup> Groups { get; set; }
        public List<BetSetting> BetSettings { get; set; }
        public List<PositionTaking> PositionTaking { get; set; }

        public AsianCommissionPlan(decimal? minBetAmount)
        {
            Groups = new List<CommissionGroup>
            {
                new CommissionGroup { Id = 1, Value = 0.0025m },
                new CommissionGroup { Id = 2, Value = 0.005m },
                new CommissionGroup { Id = 3, Value = 0.0075m },
                new CommissionGroup { Id = 4, Value = 0.01m },
                new CommissionGroup { Id = 5, Value = 0.0025m },
                new CommissionGroup { Id = 6, Value = 0.01m }
            };
            var minBet = minBetAmount ?? 0;
            BetSettings = new List<BetSetting>
            {
                new BetSetting { Id = 1, Name = "Soccer", MinBet = minBet, MinBetLimit = minBet,
                                                          MaxBet = minBet * 10000, MaxBetLimit = minBet * 10000,
                                                          MaxPerMatch = minBet * 2 * 10000, MaxPerMatchLimit = minBet * 2 * 10000,
                                                          IsParlay = 0, PreventBetting = false, ParentPreventBetting = false },
                new BetSetting { Id = 1, Name = "Soccer Parlay", MinBet = minBet, MinBetLimit = minBet,
                                                                 MaxBet = minBet * 10000, MaxBetLimit = minBet * 10000,
                                                                 MaxPerMatch = minBet * 2 * 10000,  MaxPerMatchLimit = minBet * 2 * 10000,
                                                                 IsParlay = 0, PreventBetting = false, ParentPreventBetting = false },
                new BetSetting { Id = 0, Name = "Other Sports", MinBet = minBet, MinBetLimit = minBet,
                                                                MaxBet = minBet * 10000, MaxBetLimit = minBet * 10000,
                                                                MaxPerMatch = minBet * 2 * 10000, MaxPerMatchLimit = minBet * 2 * 10000,
                                                                IsParlay = 0, PreventBetting = false, ParentPreventBetting = false },
                new BetSetting { Id = 0, Name = "Other Sports Parlay", MinBet = minBet,  MinBetLimit = minBet,
                                                                       MaxBet = minBet * 10000, MaxBetLimit = minBet * 10000,
                                                                       MaxPerMatch = minBet * 2 * 10000, MaxPerMatchLimit = minBet * 2 * 10000,
                                                                       IsParlay = 0, PreventBetting = false, ParentPreventBetting = false }
            };

            PositionTaking = new List<PositionTaking>();
            PositionTaking.Add(new PositionTaking
            {
                SportId = 1,
                MarketTypes = new List<MarketType>
                {
                    new MarketType {
                      Id = 1004,
                      AgentPercent = 0.8m,
                      AutoPT = false,
                      OwnerPercent = 0.2m,
                      Name = "Handicap",
                      IsLive = 0
                    },
                    new MarketType {
                      Id = 1005,
                      AgentPercent = 0.8m,
                      AutoPT = false,
                      OwnerPercent = 0.2m,
                      Name = "Over/Under",
                      IsLive = 0
                    },
                    new MarketType {
                      Id = 1009,
                      AgentPercent = 0.8m,
                      AutoPT = false,
                      OwnerPercent = 0.2m,
                      Name = "1stHdp",
                      IsLive = 0
                    },
                    new MarketType {
                      Id = 1010,
                      AgentPercent = 0.8m,
                      AutoPT = false,
                      OwnerPercent = 0.2m,
                      Name = "1stOU",
                      IsLive = 0
                    },
                    new MarketType {
                      Id = 0,
                      AgentPercent = 0.8m,
                      AutoPT = false,
                      OwnerPercent = 0.2m,
                      Name = "Others",
                      IsLive = 0
                    },
                    new MarketType {
                      Id = 1003,
                      AgentPercent = 0.8m,
                      AutoPT = false,
                      OwnerPercent = 0.2m,
                      Name = "1x2",
                      IsLive = 0
                    },
                    new MarketType {
                      Id = -2,
                      AgentPercent = 0.8m,
                      AutoPT = false,
                      OwnerPercent = 0.2m,
                      Name = "Outright",
                      IsLive = 0
                    },
                    new MarketType {
                      Id = 1004,
                      AgentPercent = 0.8m,
                      AutoPT = false,
                      OwnerPercent = 0.2m,
                      Name = "Handicap",
                      IsLive = 1
                    },
                    new MarketType {
                      Id = 1005,
                      AgentPercent = 0.8m,
                      AutoPT = false,
                      OwnerPercent = 0.2m,
                      Name = "Over/Under",
                      IsLive = 1
                    },
                    new MarketType {
                      Id = 1009,
                      AgentPercent = 0.8m,
                      AutoPT = false,
                      OwnerPercent = 0.2m,
                      Name = "1stHdp",
                      IsLive = 1
                    },
                    new MarketType {
                      Id = 1010,
                      AgentPercent = 0.8m,
                      AutoPT = false,
                      OwnerPercent = 0.2m,
                      Name = "1stOU",
                      IsLive = 1
                    },
                    new MarketType {
                      Id = 0,
                      AgentPercent = 0.8m,
                      AutoPT = false,
                      OwnerPercent = 0.2m,
                      Name = "Others",
                      IsLive = 1
                    },
                    new MarketType {
                      Id = 1003,
                      AgentPercent = 0.8m,
                      AutoPT = false,
                      OwnerPercent = 0.2m,
                      Name = "1x2",
                      IsLive = 1
                    },
                    new MarketType {
                      Id = -2,
                      AgentPercent = 0.8m,
                      AutoPT = false,
                      OwnerPercent = 0.2m,
                      Name = "Outright",
                      IsLive = 1
                    }
                }
            });
            PositionTaking.Add(new PositionTaking
            {
                SportId = 0,
                MarketTypes = new List<MarketType>
                {
                    new MarketType {
                      Id = 1004,
                      AgentPercent = 0.8m,
                      AutoPT = false,
                      OwnerPercent = 0.2m,
                      Name = "Handicap"
                    },
                    new MarketType {
                      Id = 1005,
                      AgentPercent = 0.8m,
                      AutoPT = false,
                      OwnerPercent = 0.2m,
                      Name = "Over/Under"
                    },
                    new MarketType {
                      Id = -2,
                      AgentPercent = 0.8m,
                      AutoPT = false,
                      OwnerPercent = 0.2m,
                      Name = "Outright"
                    },
                    new MarketType {
                      Id = 0,
                      AgentPercent = 0.8m,
                      AutoPT = false,
                      OwnerPercent = 0.2m,
                      Name = "Others"
                    }
                }
            });
            PositionTaking.Add(new PositionTaking
            {
                SportId = -1,
                MarketTypes = new List<MarketType>
                {
                    new MarketType {
                      Id = -1,
                      AgentPercent = 0.8m,
                      AutoPT = false,
                      OwnerPercent = 0.2m,
                      Name = "MixParlay"
                    },
                    new MarketType {
                      Id = 0,
                      AgentPercent = 0.8m,
                      AutoPT = false,
                      OwnerPercent = 0.2m,
                      Name = "Others"
                    }
                }
            });
        }
    }

    public class CommissionGroup
    {
        public int Id { get; set; }
        public decimal Value { get; set; }
        public decimal? ParentValue { get; set; }
    }

    public class BetSetting
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

    public class PositionTaking
    {
        public int SportId { get; set; }
        public List<MarketType> MarketTypes { get; set; }
    }

    public class MarketType
    {
        public int Id { get; set; }
        public decimal AgentPercent { get; set; }
        public bool AutoPT { get; set; }
        public decimal OwnerPercent { get; set; }
        public string Name { get; set; }
        public int? IsLive { get; set; }
    }
}
