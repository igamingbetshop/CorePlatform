using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.Bonus
{
    public class BonusTicketInfo
    {
        public int SelectionsCount { get; set; }
        public int BetType { get; set; }
        public decimal Price { get; set; }
        public decimal BetAmount { get; set; }
        public decimal WinAmount { get; set; }
        public int NumberOfWonSelections { get; set; }
        public int NumberOfLostSelections { get; set; }
        public int BetStatus { get; set; }
        public bool ToBonusBalance { get; set; }
        public decimal BonusAmount { get; set; }
        public List<BonusTicketSelection> BetSelections { get; set; }
    }

    public class BonusTicketSelection
    {
        public int SportId { get; set; }
        public int RegionId { get; set; }
        public string CompetitionId { get; set; }
        public string MatchId { get; set; }
        public long MarketId { get; set; }
        public string SelectionId { get; set; }
        public int MarketTypeId { get; set; }
        public int SelectionTypeId { get; set; }
        public int MatchStatus { get; set; }
        public decimal Price { get; set; }
    }
}
