using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.RelaxGaming
{
    public class RollbackInput
    {
        [JsonProperty(PropertyName = "customerid")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "gamesessionid")]
        public string RoundId { get; set; }

        [JsonProperty(PropertyName = "txid")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "originaltxid")]
        public string OriginalTransactionId { get; set; }
    }
}