namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class CashDeskCorrectionInput : RequestBase
    {
        public decimal Amount { get; set; }
        public string CurrencyId { get; set; }
        public int CashDeskId { get; set; }
        public int CashierId { get; set; }
        public long ExternalOperationId { get; set; }
        public string ExternalTransactionId { get; set; }
        public string Info { get; set; }
    }
}