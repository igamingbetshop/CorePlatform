using System;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.Bonus
{
    public class FreeSpinModel
    {
        public int ClientId { get; set; }
        public int BonusId { get; set; }
        public string ProductExternalId { get; set; }
        public int ProductId { get; set; }
        public List<string> ProductExternalIds { get; set; }
        public int? SpinCount { get; set; }
        public decimal? Lines { get; set; }
        public decimal? Coins { get; set; }
        public decimal? CoinValue { get; set; }
        public decimal? BetValueLevel { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }

    }
}
