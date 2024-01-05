using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.LuckyGaming
{
    public class BalanceOutput
    {
        [JsonProperty(PropertyName = "account")]
        public string AccountName { get; set; }

        [JsonProperty(PropertyName = "agent")]
        public string AgentID { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "errorCode")]
        public string ErrorCode { get; set; }
    }
}