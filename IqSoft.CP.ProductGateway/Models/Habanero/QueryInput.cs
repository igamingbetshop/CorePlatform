
using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Habanero
{
    public class QueryInput
    {
        [JsonProperty(PropertyName = "transferid")]
        public string TransferId { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "gameinstanceid")]
        public string GameInstanceId { get; set; }

        [JsonProperty(PropertyName = "accountid")]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "queryamount")]
        public decimal QueryAmount { get; set; }

    }
}