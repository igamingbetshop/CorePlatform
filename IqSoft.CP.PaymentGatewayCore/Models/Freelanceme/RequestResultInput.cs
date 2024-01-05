using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Freelanceme
{
    public class RequestResultInput
    {

        [JsonProperty(PropertyName = "auth")]
        public Authorization AuthorizationData { get; set; }

        [JsonProperty(PropertyName = "transaction_id")]
        public string ExternalTransactionId { get; set; }

        [JsonProperty(PropertyName = "external_transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "new_status")]
        public int Status { get; set; }
    }

    public class Authorization
    {
        [JsonProperty(PropertyName = "login")]
        public string Login { get; set; }

        [JsonProperty(PropertyName = "salt")]
        public string Salt { get; set; }

        [JsonProperty(PropertyName = "hash")]
        public string Hash { get; set; }
    }
}