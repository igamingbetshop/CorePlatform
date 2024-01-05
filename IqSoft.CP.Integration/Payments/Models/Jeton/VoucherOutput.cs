using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Jeton
{
    internal class VoucherOutput
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "reference")]
        public string Reference { get; set; }

        [JsonProperty(PropertyName = "actualAmount")]
        public string ActualAmount { get; set; }

        [JsonProperty(PropertyName = "actualCurrency")]
        public string ActualCurrency { get; set; }
    }
}