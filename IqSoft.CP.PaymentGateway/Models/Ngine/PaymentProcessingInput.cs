namespace IqSoft.CP.PaymentGateway.Models.Ngine
{
    public class PaymentProcessingInput
    {
            public string OrderId { get; set; }
            public string RedirectUrl { get; set; }
            public string VerificationCode { get; set; }
            public string ExpiryMonth { get; set; }
            public string ExpiryYear { get; set; }
            public string HolderName { get; set; }
            public string CardNumber { get; set; }
            public string Zip { get; set; }
            public string City { get; set; }
            public string Country { get; set; }
            public string Address { get; set; }
    }
}