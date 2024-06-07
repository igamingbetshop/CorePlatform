using System;

namespace IqSoft.CP.Integration.Platforms.Models.KRA
{
    public class BetItem
    {
        public long BetId { get; set; }
        public int ClientId { get; set; }
        public string MobileNumber { get; set; }
        public bool IsSport { get; set; }
        public decimal BetAmount { get; set; }
        public decimal WinAmount { get; set; }
        public decimal Coefficent { get; set; }
        public decimal CurrentBalance { get; set; }
        public DateTime PlacementTime { get; set; }
        public DateTime? CalculationDate { get; set; }
    }
}
