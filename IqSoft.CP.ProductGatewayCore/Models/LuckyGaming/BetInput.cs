using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.LuckyGaming
{
    public class BetInput
    {
        [JsonProperty(PropertyName = "account")]
        public string AccountName { get; set; }

        [JsonProperty(PropertyName = "agent")]
        public string AgentID { get; set; }

        [JsonProperty(PropertyName = "coin")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "externalTransNo")]
        public string ExternalTransactionId { get; set; }
        
        [JsonProperty(PropertyName = "betId")]
        public string BetId  { get; set; }

        [JsonProperty(PropertyName = "gid")]
        public string GameId { get; set; }

    }
}