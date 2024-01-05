using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Interac
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "transactionid")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "site")]
        public string Site { get; set; }

        [JsonProperty(PropertyName = "userIp")]
        public string UserIp { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "sandbox")]
        public string Sandbox { get; set; }

        [JsonProperty(PropertyName = "entityId")]
        public string EntityId { get; set; }

        [JsonProperty(PropertyName = "iat")]
        public string Iat { get; set; }

        [JsonProperty(PropertyName = "aggid")]
        public string Aggid { get; set; }

        [JsonProperty(PropertyName = "cookie")]
        public string Cookie { get; set; }

        [JsonProperty(PropertyName = "cpiType")]
        public string CpiType { get; set; }
    }
}