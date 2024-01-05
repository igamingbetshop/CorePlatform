using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.KazPost
{
   public class PaymentRequestInput
    {
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "back_link")]
        public string BackUrl { get; set; }

        [JsonProperty(PropertyName = "payment_webhook")]
        public string NotifyUrl { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }
    }
}
