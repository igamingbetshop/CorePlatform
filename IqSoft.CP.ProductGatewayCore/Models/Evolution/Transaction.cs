using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Evolution
{
    public class Transaction
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "refId")]
        public string RefId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }
    }
}
