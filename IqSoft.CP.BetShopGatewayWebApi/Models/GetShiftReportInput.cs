namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetShiftReportInput : ApiRequestBase
    {
        public int CashDeskId { get; set; }

        public int? CashierId { get; set; }
    }
}