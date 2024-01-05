using IqSoft.CP.DAL.Models;
using IqSoft.CP.AdminWebApi.Models.ReportModels.BetShop;

namespace IqSoft.CP.AdminWebApi.Models.BetShopModels
{
    public class BetshopBetsReportModel : PagedModel<BetShopBetModel>
    {
        public decimal? TotalBetAmount { get; set; }

        public decimal? TotalWinAmount { get; set; }

        public decimal? TotalProfit { get; set; }
    }
}