using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.BetMakers
{
    public class BaseInput
    {
        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "betId")]
        public string BetId { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "bonusFlag")]
        public bool BonusFlag { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public MetadataModel Metadata { get; set; }

        [JsonProperty(PropertyName = "transactionType")]
        public string TransactionType { get; set; }
    }

    public class MetadataModel
    {
        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "brandName")]
        public string BrandName { get; set; }

        [JsonProperty(PropertyName = "brandUserId")]
        public string BrandUserId { get; set; }

        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }
    }
}