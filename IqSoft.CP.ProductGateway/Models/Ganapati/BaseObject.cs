using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Ganapati
{
    public class BaseObject
    {
        [JsonProperty(PropertyName = "launchToken")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public decimal? Balance { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "playerId")]
        public string PlayerId { get; set; }

        [JsonProperty(PropertyName = "sessionId")]
        public string RefreshToken { get; set; }

        [JsonProperty(PropertyName = "account")]
        public Account AccountData { get; set; }

        [JsonProperty(PropertyName = "errorCode")]
        public int? ErrorCode { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "details")]
        public object Details { get; set; }
    }

    public class Account
    {
        [JsonProperty(PropertyName = "country")]
        public string CountryCode { get; set; }

        [JsonProperty(PropertyName = "gender")]
        public string Gender { get; set; }

        [JsonProperty(PropertyName = "alias")]
        public string Alias { get; set; }
    }
}