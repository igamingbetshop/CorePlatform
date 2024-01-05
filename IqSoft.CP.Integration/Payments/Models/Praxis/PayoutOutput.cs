using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Praxis
{
    public class PayoutOutput
    {
        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "transaction")]
        public TransactionModel Transaction { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; } = "1.3";

        [JsonProperty(PropertyName = "timestamp")]
        public long Timestamp { get; set; }
    }
}
