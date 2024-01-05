namespace IqSoft.CP.Integration.Payments.Models.Qaicash
{
    public class PayoutOutput
    {
        public string OrderId { get; set; }
        public string TransactionId { get; set; }
        public string PayoutMethod { get; set; }
        public string DateCreated { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string DateUpdated { get; set; }
        public string UserId { get; set; }
        public string Notes { get; set; }
        public int InstrumentId { get; set; }
        public string MessageAuthenticationCode { get; set; }
    }

}
