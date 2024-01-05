using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.NodaPay
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}
