using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Rocabee
{
    public class BaseInput
    {
        [JsonProperty(PropertyName = "userId")]
        public int ClientId { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "operationType")]
        public string OperationType { get; set; }

        [JsonProperty(PropertyName = "info")]
        public string Info { get; set; }
    }
}