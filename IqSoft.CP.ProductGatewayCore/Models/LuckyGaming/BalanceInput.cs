using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.LuckyGaming
{
    public class BalanceInput
    {
        [JsonProperty(PropertyName = "account")]
        public string AccountName { get; set; }

        [JsonProperty(PropertyName = "agent")]
        public string AgentID { get; set; }
    }
}