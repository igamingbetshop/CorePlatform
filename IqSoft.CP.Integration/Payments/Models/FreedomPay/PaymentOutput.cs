using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.FreedomPay
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string RedirectUrl { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}
