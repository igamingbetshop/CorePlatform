namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class ApiGetCashierSessionInput : RequestBase
    {
        public int CashDeskId { get; set; }

        public string SessionToken { get; set; }
    }
}