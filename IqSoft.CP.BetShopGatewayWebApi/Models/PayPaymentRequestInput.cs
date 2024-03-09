namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class PayPaymentRequestInput 
    {
        public int CashierId { get; set; }
        public int PaymentRequestId { get; set; }
        public string Comment { get; set; }
    }
}