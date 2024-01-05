namespace IqSoft.CP.PaymentGateway.Models.PaymentProcessing
{
    public class ResultOutput
    {
        public int  StatusCode { get; set; } = 0;
        public string  RedirectUrl { get; set; }
    }
}