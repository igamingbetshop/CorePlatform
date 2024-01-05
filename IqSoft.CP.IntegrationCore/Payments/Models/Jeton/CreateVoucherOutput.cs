using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Payments.Models.Jeton
{
    public class CreateVoucherOutput
    {
        [JsonProperty(PropertyName = "voucherNumber")]
        public string VoucherNumber { get; set; }

        [JsonProperty(PropertyName = "secureCode")]
        public string SecureCode { get; set; }

        [JsonProperty(PropertyName = "expiryMonth")]
        public int ExpiryMonth { get; set; }

        [JsonProperty(PropertyName = "expiryYear")]
        public int ExpiryYear { get; set; }

        [JsonProperty(PropertyName = "issueDate")]
        public DateTime IssueDate { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "reference")]
        public string Reference { get; set; }

        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}
