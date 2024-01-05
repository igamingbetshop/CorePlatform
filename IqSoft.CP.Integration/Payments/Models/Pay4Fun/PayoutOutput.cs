namespace IqSoft.CP.Integration.Payments.Models.Pay4Fun
{
    public class PayoutOutput
    {
        public string TransactionId { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public decimal FeeAmount { get; set; }
        public long MerchantInvoiceId { get; set; }
        public string OriginalCurrency { get; set; }
        public decimal OriginalAmount { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string CustomerEmail { get; set; }
        public string Sign { get; set; }
    }
}
