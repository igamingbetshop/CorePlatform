using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.GoldenRace
{ 
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "status")]
        public bool Status { get; set; } = true;

        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; } = 200;

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; } = "Success";

        [JsonProperty(PropertyName = "data")]
        public DataModel Data { get; set; }
    }

    public class DataModel
    {
        [JsonProperty(PropertyName = "playerId")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "playerNickname")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "oldBalance")]
        public decimal? OldBalance { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "sessionId")]
        public string SessionId { get; set; }

        [JsonProperty(PropertyName = "group")]
        public string GroupName { get; set; } = "master";

        [JsonProperty(PropertyName = "timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty(PropertyName = "requestId")]
        public string RequestId { get; set; }

        [JsonProperty(PropertyName = "siteId")]
        public string SiteId { get; set; }

        [JsonProperty(PropertyName = "fingerprint")]
        public string Fingerprint { get; set; }
    }
}