namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class PayWinInput : RequestBase
    {
        public int CashDeskId { get; set; }

        public int CashierId { get; set; }

        public long BetDocumentId { get; set; }

        public string ExternalTransactionId { get; set; }
    }
}