namespace IqSoft.CP.PaymentGateway.Models.Eway
{
    public class PaymentResultOutput
    {
        public string AccessCode { get; set; }
        public string AuthorisationCode { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceReference { get; set; }
        public decimal TotalAmount { get; set; }
        public string TransactionID { get; set; }
        public bool TransactionStatus { get; set; }
        public BeagleVerificationModel BeagleVerification { get; set; }
        public object Errors { get; set; }
    }
    public class BeagleVerificationModel
    {
        public int? Email { get; set; }
        public int? Phone { get; set; }
    }

}