using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Capital
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
        
        [JsonProperty(PropertyName = "order_id")]
        public string ExternalTransactionId { get; set; }

        [JsonProperty(PropertyName = "request_id")]
        public string RequestId { get; set; }

        [JsonProperty(PropertyName = "redirect_url")]
        public string RedirectUrl { get; set; }

    }
}
