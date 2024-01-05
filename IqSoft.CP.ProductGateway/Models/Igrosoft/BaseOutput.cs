using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Igrosoft
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "sessionId")]
        public string SessionId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "originalTrxId")]
        public string OriginalTrxId { get; set; }
    }
}