namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetCashDeskCurrentBalanceOutput : ApiResponseBase
    {
        public decimal Balance { get; set; }

        public decimal CurrentLimit { get; set; }
    }
}