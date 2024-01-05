using IqSoft.CP.Common.Enums;

namespace IqSoft.CP.Integration.Payments.Models
{
    public class PaymentResponse
    {
        public string Url { get; set; }
        public string CancelUrl { get; set; }
        public int Type { get; set; }
        public string Description { get; set; }
        public PaymentRequestStates Status { get; set; }
        public string Data { get; set; }
    }
}