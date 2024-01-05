using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.EvenBet
{
    public class BaseInput
    {
        [JsonProperty(PropertyName ="method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName ="amount")]
        public decimal? Amount { get; set; }

        [JsonProperty(PropertyName = "sessionId")]
        public string SessionId { get; set; }

        [JsonProperty(PropertyName ="transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "referenceTransactionId")]
        public string ReferenceTransactionId { get; set; }

        [JsonProperty(PropertyName = "rake")]
        public decimal? Rake { get; set; }
    }
}