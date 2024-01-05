namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetCashDeskOperationsInput : ApiRequestBase
    {
        public int? LastShiftsNumber { get; set; }

        public int CashierId { get; set; }

        public int CashDeskId { get; set; }
    }
}