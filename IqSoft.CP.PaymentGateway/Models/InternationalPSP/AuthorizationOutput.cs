using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.InternationalPSP
{
    public class AuthorizationOutput
    {
        [JsonProperty(PropertyName = "journal")]
        public string Journal { get; set; }

        [JsonProperty(PropertyName = "transaction_uuid")]
        public string TransactionUuid { get; set; }

        [JsonProperty(PropertyName = "operation_uuid")]
        public string OperationUuid { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "code")]
        public string StatusCode { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "amount_authorized")]
        public decimal AmountAuthorized { get; set; }

        [JsonProperty(PropertyName = "amount_captured")]
        public decimal AmountCaptured { get; set; }

        [JsonProperty(PropertyName = "redirect_url")]
        public string RedirectUrl { get; set; }

        [JsonProperty(PropertyName = "signature")]
        public string Signature { get; set; }
    }
}