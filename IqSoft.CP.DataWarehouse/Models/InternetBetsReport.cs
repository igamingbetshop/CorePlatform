using IqSoft.CP.Common.Models;
namespace IqSoft.CP.DataWarehouse.Models
{
    public class InternetBetsReport : PagedModel<fnInternetBet>
    {
        public decimal? TotalBetAmount { get; set; }

        public decimal? TotalWinAmount { get; set; }

        public decimal? TotalPossibleWinAmount { get; set; }

        public int? TotalCurrencyCount { get; set; }

        public int? TotalPlayersCount { get; set; }

        public int? TotalProvidersCount { get; set; }

        public int? TotalProductsCount { get; set; }

		public decimal? TotalGGR { get; set; }
    }
}
