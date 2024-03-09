using System;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.DashboardModels
{
    public class ApiProvidersBetsInfo
    {
        public int TotalPlayersCount { get; set; }
        public decimal TotalBetsAmount { get; set; }
        public decimal TotalBonusBetsAmount { get; set; }
        public decimal TotalWinsAmount { get; set; }
        public decimal TotalBonusWinsAmount { get; set; }
        public decimal TotalGGR { get; set; }
        public decimal TotalNGR { get; set; }
        public List<ApiProviderBetsInfo> Bets { get; set; }
    }

    public class ApiProviderBetsInfo
    {
        public string GameProviderName { get; set; }
        public string SubProviderName { get; set; }
        public int GameProviderId { get; set; }
        public int SubProviderId { get; set; }
        public decimal TotalBetsAmount { get; set; }
        public decimal TotalBonusBetsAmount { get; set; }
        public decimal TotalWinsAmount { get; set; }
        public decimal TotalBonusWinsAmount { get; set; }
        public int TotalBetsCount { get; set; }
        public decimal TotalGGR { get; set; }
        public decimal TotalNGR { get; set; }
        public decimal TotalPlayersCount { get; set; }
        public decimal TotalBetsAmountFromInternet { get; set; }
        public decimal TotalBetsAmountFromBetShop { get; set; }
        public List<ApiProviderDailyInfo> DailyInfo { get; set; }
    }

    public class ApiProviderDailyInfo
    {
        public DateTime Date { get; set; }
        public string GameProviderName { get; set; }
        public string SubProviderName { get; set; }
        public int GameProviderId { get; set; }
        public int SubProviderId { get; set; }
        public decimal TotalBetsAmount { get; set; }
        public decimal TotalBonusBetsAmount { get; set; }
        public decimal TotalWinsAmount { get; set; }
        public decimal TotalBonusWinsAmount { get; set; }
        public int TotalBetsCount { get; set; }
        public decimal TotalGGR { get; set; }
        public decimal TotalNGR { get; set; }
        public decimal TotalPlayersCount { get; set; }
        public decimal TotalBetsAmountFromInternet { get; set; }
        public decimal TotalBetsAmountFromBetShop { get; set; }
    }
}