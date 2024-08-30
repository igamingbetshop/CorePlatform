namespace IqSoft.CP.PaymentGateway.Models.Mpesa
{
    public class DirectPaymentOutput
    {
        public string ResultCode { get; set; }
        public string ResultDesc { get; set; }
        public string ThirdPartyTransID { get; set; }
    }
}