namespace IqSoft.CP.PaymentGateway.Models.ExternalCashier
{
    public class AuthenticationInput
    {
        public string ClientId { get; set; }
        public string Signature { get; set; }
    }
}