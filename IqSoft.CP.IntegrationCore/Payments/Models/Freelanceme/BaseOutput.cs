using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Freelanceme
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "response")]
        public ResponseType Response { get; set; }

        [JsonProperty(PropertyName = "error")]
        public ErrorType ErrorDescription { get; set; }
    }
    public class ErrorType
    {
        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }

    public class ResponseType
    {
        [JsonProperty(PropertyName = "id")]
        public string ExternalId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "client_id")]
        public int ClientId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "external_transaction_id")]
        public int PaymentRequestId { get; set; }

        [JsonProperty(PropertyName = "pay_url")]
        public string PayUrl { get; set; }

        [JsonProperty(PropertyName = "is_test")]
        public bool IsTest { get; set; }
    }
}