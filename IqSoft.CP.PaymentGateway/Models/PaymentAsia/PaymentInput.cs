namespace IqSoft.CP.PaymentGateway.Models.PaymentAsia
{
    public class PaymentInput
    {
        public string merchant_reference { get; set; }
        public string request_reference { get; set; }
        public decimal amount { get; set; }
        public string currency { get; set; }
        public int status { get; set; }
        public string sign { get; set; }
    }
}