namespace IqSoft.CP.PaymentGateway.Models.Qaicash
{
    public class PaymentInput
    {
        public int OrderId { get; set; }
        public long TransactionId { get; set; }
        public string DateCreated { get; set; }
        public string DepositMethod { get; set; }
        public string Processor { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string DateUpdated { get; set; }
        public string DepositorUserId { get; set; }
        public string MessageAuthenticationCode { get; set; }
    }

}
