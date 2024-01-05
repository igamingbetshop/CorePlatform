using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllBet
    {
        public long ClientId { get; set; }
        public int? BetProductId { get; set; }
        public int? WinProductId { get; set; }
        public long? BetId { get; set; }
        public long? WinId { get; set; }
        public decimal? BetAmount { get; set; }
        public decimal? WinAmount { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? CalculationTime { get; set; }
    }
}
