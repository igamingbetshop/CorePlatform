using Newtonsoft.Json;
namespace IqSoft.CP.ProductGateway.Models.JackpotGaming
{
    public class DebitInput
    {
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "reference")]
        public string Reference { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "details")]
        public Details Details { get; set; }

        [JsonProperty(PropertyName = "productId")]
        public string ProductId { get; set; }

        [JsonProperty(PropertyName = "brandId")]
        public string BrandId { get; set; }


        [JsonProperty(PropertyName = "type")]
        public string TransactionType { get; set; }

    }

    public class Details
    {
        [JsonProperty(PropertyName = "request")]
        public Request Request { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "platformName")]
        public string PlatformName { get; set; }
    }

    public class Request
    {
        [JsonProperty(PropertyName = "legaSessionId")]
        public string LegaSessionId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "gameId")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "playId")]
        public string PlayId { get; set; }

        [JsonProperty(PropertyName = "operation")]
        public string Operation { get; set; }

        [JsonProperty(PropertyName = "gameMode")]
        public string GameMode { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "amountCurrency")]
        public string AmountCurrency { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }
    }
}