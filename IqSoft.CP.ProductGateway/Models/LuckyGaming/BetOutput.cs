using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.LuckyGaming
{
    public class BetOutput
    {
        [JsonProperty(PropertyName = "afterBalance")]
        public decimal AfterBalance { get; set; }

        [JsonProperty(PropertyName = "agent")]
        public string AgentID { get; set; }

        [JsonProperty(PropertyName = "beforeBalance")]
        public decimal BeforeBalance { get; set; }

        [JsonProperty(PropertyName = "coin")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "gid")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "externalTransNo")]
        public string ExternalTransactionId { get; set; }

        [JsonProperty(PropertyName = "resultCode")]
        public int ResultCode { get; set; }

        [JsonProperty(PropertyName = "betId")]
        public string BetID { get; set; }

        [JsonProperty(PropertyName = "errorCode")]
        public string ErrorCode { get; set; }
    }
}