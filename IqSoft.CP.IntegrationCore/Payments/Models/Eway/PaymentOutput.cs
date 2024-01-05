namespace IqSoft.CP.Integration.Payments.Models.Eway
{
    public class PaymentOutput
    {
        public string SharedPaymentUrl { get; set; }
        public string AccessCode { get; set; }
        public string Errors { get; set; }
    }
}
