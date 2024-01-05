using IqSoft.CP.Common.Models;
namespace IqSoft.CP.AgentWebApi.Models
{
    public class BetshopBetsReportModel : PagedModel<BetShopBetModel>
    {
        public decimal? TotalBetAmount { get; set; }

        public decimal? TotalWinAmount { get; set; }

        public decimal? TotalProfit { get; set; }
    }
}