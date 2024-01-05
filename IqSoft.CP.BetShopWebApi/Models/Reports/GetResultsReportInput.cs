using IqSoft.CP.BetShopGatewayWebApi.Models;

namespace IqSoft.CP.BetShopWebApi.Models.Reports
{
    public class GetResultsReportInput : ApiFilterBase
    {
        public int? GameId { get; set; }
    }
}