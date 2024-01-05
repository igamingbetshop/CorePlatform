using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Dashboard
{
    public class ProvidersBetsInfo
    {
        public int TotalPlayersCount { get; set; }
        public decimal TotalBetsAmount { get; set; }
        public decimal TotalWinsAmount { get; set; }
        public decimal TotalGGR { get; set; }
        public List<ProviderBetsInfo> Bets { get; set; }
    }

    public class ProviderBetsInfo
    {
        public int ProviderId { get; set; }

	    public int DocumentState { get; set; }

	    public string CurrencyId { get; set; }

        public decimal TotalBetsAmount { get; set; }

        public decimal TotalWinsAmount { get; set; }

        public int TotalBetsCount { get; set; }

        public decimal TotalGGR { get; set; }

        public int TotalPlayersCount { get; set; }

        public decimal TotalBetsAmountFromInternet { get; set; }

        public decimal TotalBetsAmountFromBetShop { get; set; }
    }
}