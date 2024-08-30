﻿using System;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.DashboardModels
{
    public class ApiPlayersInfo
    {
        public int VisitorsCount { get; set; }
        public int SignUpsCount { get; set; }
        public int TotalPlayersCount { get; set; }
        public int ReturnsCount { get; set; }
        public int DepositsCount { get; set; }
        public decimal TotalBonusAmount { get; set; }
        public decimal TotalCashoutAmount { get; set; }
        public decimal TotalBetAmount { get; set; }
        public decimal AverageBet { get; set; }
        public decimal MaxBet { get; set; }
        public decimal MaxWin { get; set; }
        public decimal MaxWinBet { get; set; }
        public int FTDCount { get; set; }
        public List<ApiPlayersDailyInfo> DailyInfo { get; set; }
    }
    public class ApiPlayersDailyInfo
    {
        public DateTime Date { get; set; }
        public int VisitorsCount { get; set; }
        public int SignUpsCount { get; set; }
        public int TotalBetsCount { get; set; }
        public int TotalPlayersCount { get; set; }
        public int ReturnsCount { get; set; }
        public int DepositsCount { get; set; }
        public decimal TotalBonusAmount { get; set; }
        public decimal TotalCashoutAmount { get; set; }
        public decimal TotalBetAmount { get; set; }
        public decimal AverageBet { get; set; }
        public decimal MaxBet { get; set; }
        public decimal MaxWin { get; set; }
        public decimal MaxWinBet { get; set; }
        public int FTDCount { get; set; }
    }
}