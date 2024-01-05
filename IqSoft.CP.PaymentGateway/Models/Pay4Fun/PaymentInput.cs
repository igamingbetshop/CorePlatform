namespace IqSoft.CP.PaymentGateway.Models.Pay4Fun
{
    public class PaymentInput
    {
        public int TransactionId { get; set; }
        public decimal Amount { get; set; }
        public decimal FeeAmount { get; set; }
        public int MerchantInvoiceId { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string LiquidationDate { get; set; }
        public string Message { get; set; }
        public string CustomerEmail { get; set; }
        public string Sign { get; set; }
    }
}