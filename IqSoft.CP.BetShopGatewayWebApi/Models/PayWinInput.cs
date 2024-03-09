namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class PayWinInput 
    {
        public int CashierId { get; set; }

        public long BetDocumentId { get; set; }

        public string ExternalTransactionId { get; set; }
    }
}