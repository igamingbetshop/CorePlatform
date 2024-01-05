namespace IqSoft.CP.PaymentGateway.Models.ExternalCashier
{
    public class PaymentInput
    {
        public string ClientId { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; }
        public string Signature { get; set; }
    }
}