using System;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.DashboardModels
{
    public class ApiBetsInfo
    {
        public decimal TotalBetsAmount { get; set; }
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
        public decimal TotalBetsCountFromTablet { get; set; }
        public decimal TotalBetsFromTablet { get; set; }
        public decimal TotalGGRFromTablet { get; set; }
        public decimal TotalNGRFromTablet { get; set; }
        public decimal TotalPlayersCountFromTablet { get; set; }
        public List<ApiBetsDailyInfo> DailyInfo { get; set; }
    }

    public class ApiBetsDailyInfo
    {
        public DateTime Date { get; set; }
        public decimal TotalBetsAmount { get; set; }
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
        public decimal TotalBetsCountFromTablet { get; set; }
        public decimal TotalBetsFromTablet { get; set; }
        public decimal TotalGGRFromTablet { get; set; }
        public decimal TotalNGRFromTablet { get; set; }
        public decimal TotalPlayersCountFromTablet { get; set; }
    }
}