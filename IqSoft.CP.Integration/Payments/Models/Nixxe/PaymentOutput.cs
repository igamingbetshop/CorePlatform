using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Nixxe
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "url")]
        public string RedirectUrl { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }
    }
}
