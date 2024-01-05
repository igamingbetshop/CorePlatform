using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.Bonus
{
    public class BonusCondition
    {
        public int GroupingType { get; set; }
        public List<BonusConditionItem> Conditions { get; set; }
        public List<BonusConditionGroup> Groups { get; set; }
    }

    public class BonusConditionGroup
    {
        public int GroupingType { get; set; }
        public List<BonusConditionItem> Conditions { get; set; }
        public List<BonusConditionSubGroup> Groups { get; set; }
    }

    public class BonusConditionSubGroup
    {
        public int GroupingType { get; set; }
        public List<BonusConditionItem> Conditions { get; set; }
    }

    public class BonusConditionItem
    {
        public int ConditionType { get; set; }
        public int OperationTypeId { get; set; }
        public string StringValue { get; set; }
    }

     public enum BonusConditionTypes
    {
        Sport = 1,
        Region = 2,
        Competition = 3,
        Match = 4,
        Market = 5,
        Selection = 6,
        MarketType = 7,
        SelectionType = 8,
        MatchStatus = 9,
        Price = 10,
        PricePerSelection = 11,
        BetType = 12,
        NumberOfSelections = 13,
        NumberOfWonSelections = 14,
        NumberOfLostSelections = 15,
        Stake = 16,
        BetStatus = 17
    }
}
