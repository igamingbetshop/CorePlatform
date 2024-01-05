using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.DashboardModels
{
    public class ApiProvidersBetsInfo
    {
        public int TotalPlayersCount { get; set; }
        public decimal TotalBetsAmount { get; set; }
        public decimal TotalWinsAmount { get; set; }
        public decimal TotalGGR { get; set; }
        public List<ApiProviderBetsInfo> Bets { get; set; }
    }

    public class ApiProviderBetsInfo
    {
        public int ProviderId { get; set; }

        public decimal TotalBetsAmount { get; set; }

        public decimal TotalWinsAmount { get; set; }

        public int TotalBetsCount { get; set; }

        public decimal TotalGGR { get; set; }

        public decimal TotalPlayersCount { get; set; }

        public decimal TotalBetsAmountFromInternet { get; set; }

        public decimal TotalBetsAmountFromBetShop { get; set; }
    }
}