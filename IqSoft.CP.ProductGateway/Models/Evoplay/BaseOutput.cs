using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Evoplay
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "data")]
        public Details Data { get; set; }

        [JsonProperty(PropertyName = "error")]
        public ErrorDetails Error { get; set; }
    }

    public class Details
    {
        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }
    }

    public class ErrorDetails
    {
        [JsonProperty(PropertyName = "scope")]
        public string Scope { get; set; }

        [JsonProperty(PropertyName = "no_refund")]
        public string NoRefund { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}