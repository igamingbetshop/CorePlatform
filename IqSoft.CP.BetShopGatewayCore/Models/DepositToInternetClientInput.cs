namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class DepositToInternetClientInput : RequestBase
    {
        public int ClientId { get; set; }
        public string TransactionId { get; set; }
        public int CashierId { get; set; }
        public int CashDeskId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyId { get; set; }
    }
}