using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllRealTimeInfo
    {
        public int? PartnerId { get; set; }
        public int? LoginCount { get; set; }
        public int? BetsCount { get; set; }
        public decimal? BetsAmount { get; set; }
        public int? PlayersCount { get; set; }
        public int? ApprovedDepositsCount { get; set; }
        public decimal? ApprovedDepositsAmount { get; set; }
        public int? ApprovedWithdrawalsCount { get; set; }
        public decimal? ApprovedWithdrawalsAmount { get; set; }
        public int? WonBetsCount { get; set; }
        public decimal? WonBetsAmount { get; set; }
        public int? LostBetsCount { get; set; }
        public decimal? LostBetsAmount { get; set; }
    }
}
