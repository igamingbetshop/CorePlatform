namespace IqSoft.CP.PaymentGateway.Models.AsPay
{
    public class PaymentInput
    {
        public bool IsSuccess { get; set; }
        public string UniqueIdentifier { get; set; }
        public string TransactionGuid { get; set; }
        public string ProviderTransactionId { get; set; }
        public string OrderId { get; set; }
        public string Hash { get; set; }
    }
}