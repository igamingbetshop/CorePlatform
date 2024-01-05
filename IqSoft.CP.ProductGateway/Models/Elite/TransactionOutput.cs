using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Elite
{
    public class TransactionOutput
    {
        [JsonProperty(PropertyName = "cash")]
        public decimal Cash { get; set; }

        [JsonProperty(PropertyName = "bonus")]
        public decimal Bonus { get; set; }

        [JsonProperty(PropertyName = "externalTransactionId")]
        public string ExternalTransactionId { get; set; }
    }
}