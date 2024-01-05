namespace IqSoft.CP.PaymentGateway.Models.PayOne
{
    public class PaymentRequestOutput
    {
        public int Status { get; set; }
        public int Amount { get; set; }
        public string TrackingNumber { get; set; }
        public string CardNumber { get; set; }
        public string HolderName { get; set; }
        public string Description { get; set; }
    }
}