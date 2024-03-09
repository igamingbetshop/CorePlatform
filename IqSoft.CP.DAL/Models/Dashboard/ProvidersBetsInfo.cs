using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Dashboard
{
    public class ProvidersBetsInfo
    {
        public int TotalPlayersCount { get; set; }
        public decimal TotalBetsAmount { get; set; }
        public decimal TotalBonusBetsAmount { get; set; }
        public decimal TotalWinsAmount { get; set; }
        public decimal TotalBonusWinsAmount { get; set; }
        public decimal TotalGGR { get; set; }
        public decimal TotalNGR { get; set; }
        public List<ProviderBetsInfo> Bets { get; set; }
    }

    public class ProviderBetsInfo
    {
        public string GameProviderName { get; set; }
        public string SubProviderName { get; set; }
        public int GameProviderId { get; set; }
        public int SubProviderId { get; set; }
        public int DocumentState { get; set; }
	    public string CurrencyId { get; set; }
        public decimal TotalBetsAmount { get; set; }
        public decimal TotalBonusBetsAmount { get; set; }
        public decimal TotalWinsAmount { get; set; }
        public decimal TotalBonusWinsAmount { get; set; }
        public int TotalBetsCount { get; set; }
        public decimal TotalGGR { get; set; }
        public decimal TotalNGR { get; set; }
        public int TotalPlayersCount { get; set; }
        public decimal TotalBetsAmountFromInternet { get; set; }
        public decimal TotalBetsAmountFromBetShop { get; set; }
        public List<ProviderDailyInfo> DailyInfo { get; set; }
    }

    public class ProviderDailyInfo
    {
        public long LongDate { get; set; }
        public DateTime Date { get; set; }
        public string GameProviderName { get; set; }
        public string SubProviderName { get; set; }
        public int GameProviderId { get; set; }
        public int SubProviderId { get; set; }
        public int DocumentState { get; set; }
        public string CurrencyId { get; set; }
        public decimal TotalBetsAmount { get; set; }
        public decimal TotalBonusBetsAmount { get; set; }
        public decimal TotalWinsAmount { get; set; }
        public decimal TotalBonusWinsAmount { get; set; }
        public int TotalBetsCount { get; set; }
        public decimal TotalGGR { get; set; }
        public decimal TotalNGR { get; set; }
        public int TotalPlayersCount { get; set; }
        public decimal TotalBetsAmountFromInternet { get; set; }
        public decimal TotalBetsAmountFromBetShop { get; set; }
    }
}