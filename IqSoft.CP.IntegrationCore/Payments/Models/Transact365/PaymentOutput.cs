using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Transact365
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "trans_id")]
        public string TransId { get; set; }

        [JsonProperty(PropertyName = "order_id")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty(PropertyName = "statusMessage")]
        public string StatusMessage { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}
