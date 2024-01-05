namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class CloseSessionInput : RequestBase
    {
        public int Id { get; set; }

        public int CashDeskId { get; set; }

        public int? ProductId { get; set; }
    }
}