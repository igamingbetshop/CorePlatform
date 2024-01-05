using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.LuckyGaming
{
    public class AccountInput
    {
        [JsonProperty(PropertyName = "agentID")]
        public string AgentID { get; set; }

        [JsonProperty(PropertyName = "accountName")]
        public string AccountName { get; set; }

        [JsonProperty(PropertyName = "accountPW")]
        public string AccountPW { get; set; }

        [JsonProperty(PropertyName = "accountDisplay")]
        public string AccountDisplay { get; set; }

        [JsonProperty(PropertyName = "timeStamp")]
        public long TimeStamp { get; set; }
    }
}
