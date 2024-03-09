using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Dashboard
{
    public class BetsInfo
    {
        public int DeviceTypeId { get; set; }
        public decimal TotalBetsAmount { get; set; }
        public decimal TotalBonusBetsAmount { get; set; }
        public decimal TotalWinsAmount { get; set; }
        public decimal TotalBonusWinsAmount { get; set; }
        public decimal TotalBetsCount { get; set; }
        public decimal TotalPlayersCount { get; set; }
        public decimal TotalGGR { get; set; }
        public decimal TotalNGR { get; set; }

        public decimal TotalBetsFromWebSite { get; set; }
        public decimal TotalBetsCountFromWebSite { get; set; }
        public decimal TotalPlayersCountFromWebSite { get; set; }
        public decimal TotalGGRFromWebSite { get; set; }
        public decimal TotalNGRFromWebSite { get; set; }

        public decimal TotalBetsFromMobile { get; set; }
        public decimal TotalBetsCountFromMobile { get; set; }
        public decimal TotalPlayersCountFromMobile { get; set; }
        public decimal TotalGGRFromMobile { get; set; }
        public decimal TotalNGRFromMobile { get; set; }

        public decimal TotalBetsFromTablet { get; set; }
        public decimal TotalBetsCountFromTablet { get; set; }
        public decimal TotalPlayersCountFromTablet { get; set; }
        public decimal TotalGGRFromTablet { get; set; }
        public decimal TotalNGRFromTablet { get; set; }

        public List<BetsDailyInfo> DailyInfo { get; set; }
    }

    public class BetsDailyInfo
    {
        public long LongDate { get; set; }
        public DateTime Date { get; set; }
        public int DeviceTypeId { get; set; }
        public decimal TotalBetsAmount { get; set; }
        public decimal TotalBonusBetsAmount { get; set; }
        public decimal TotalWinsAmount { get; set; }
        public decimal TotalBonusWinsAmount { get; set; }
        public decimal TotalBetsCount { get; set; }
        public decimal TotalPlayersCount { get; set; }
        public decimal TotalGGR { get; set; }
        public decimal TotalNGR { get; set; }

        public decimal TotalBetsFromWebSite { get; set; }
        public decimal TotalBetsCountFromWebSite { get; set; }
        public decimal TotalPlayersCountFromWebSite { get; set; }
        public decimal TotalGGRFromWebSite { get; set; }
        public decimal TotalNGRFromWebSite { get; set; }

        public decimal TotalBetsFromMobile { get; set; }
        public decimal TotalBetsCountFromMobile { get; set; }
        public decimal TotalPlayersCountFromMobile { get; set; }
        public decimal TotalGGRFromMobile { get; set; }
        public decimal TotalNGRFromMobile { get; set; }

        public decimal TotalBetsFromTablet { get; set; }
        public decimal TotalBetsCountFromTablet { get; set; }
        public decimal TotalPlayersCountFromTablet { get; set; }
        public decimal TotalGGRFromTablet { get; set; }
        public decimal TotalNGRFromTablet { get; set; }
    }
}