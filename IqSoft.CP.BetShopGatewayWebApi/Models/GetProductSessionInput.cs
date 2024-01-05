namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetProductSessionInput : RequestBase
    {
        public int CashDeskId { get; set; }

        public int ProviderId { get; set; }

        public int ProductId { get; set; }
    }
}