using IqSoft.CP.AdminWebApi.Models.ReportModels.BetShop;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.AdminWebApi.Models.BetShopModels
{
    public class BetshopBetsReportModel : PagedModel<BetShopBetModel>
    {
        public decimal? TotalBetAmount { get; set; }

        public decimal? TotalWinAmount { get; set; }

        public decimal? TotalProfit { get; set; }
    }
}