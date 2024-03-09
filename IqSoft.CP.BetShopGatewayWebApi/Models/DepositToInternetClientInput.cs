namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class DepositToInternetClientInput
    {
        public int ClientId { get; set; }
        public string TransactionId { get; set; }
        public int CashierId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyId { get; set; }
    }
}