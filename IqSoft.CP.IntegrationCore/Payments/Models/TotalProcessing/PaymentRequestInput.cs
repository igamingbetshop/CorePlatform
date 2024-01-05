namespace IqSoft.CP.Integration.Payments.Models.TotalProcessing
{
    public class PaymentRequestInput
    {
        public string entityId { get; set; }
        public string amount { get; set; }
        public string currency { get; set; }
        public string paymentBrand { get; set; }
        public string paymentType { get; set; }
        public string merchantTransactionId { get; set; }
        public string shopperResultUrl { get; set; }

        public string testMode { get; set; }
    }


}
