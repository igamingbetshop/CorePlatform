using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Racebook
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "error")]
        public int Error { get; set; }

        [JsonProperty(PropertyName = "msg")]
        public string ErrorDescription { get; set; }
    }
}