namespace IqSoft.CP.PaymentGateway.Models.PaySec
{
    public class PaymentRequestResultInput
    {
        public string status { get; set; }

        public string statusMessage { get; set; }

        public string cartId { get; set; }

        public string transactionReference { get; set; }

        public string currency { get; set; }

        public decimal orderAmount { get; set; }

        public string orderTime { get; set; }

        public string completedTime { get; set; }

        public string version { get; set; }

        public string signature { get; set; }
    }
}