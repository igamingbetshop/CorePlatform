﻿using System.Collections.Generic;
using IqSoft.CP.DAL.Models.Cache;

namespace IqSoft.CP.DAL.Models.RealTime
{
    public class RealTimeInfo
    {
        public List<BllOnlineClient> OnlineClients { get; set; }

        public long Count { get; set; }

        public int TotalLoginsCount { get; set; }

        public int TotalBetsCount { get; set; }

        public decimal TotalBetsAmount { get; set; }

        public int TotalPlayersCount { get; set; }

        public int ApprovedDepositsCount { get; set; }

        public decimal ApprovedDepositsAmount { get; set; }

        public int ApprovedWithdrawalsCount { get; set; }

        public decimal ApprovedWithdrawalsAmount { get; set; }

        public int WonBetsCount { get; set; }

        public decimal WonBetsAmount { get; set; }

        public int LostBetsCount { get; set; }

        public decimal LostBetsAmount { get; set; }
    }
}
