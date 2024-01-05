using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.MaldoPay
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "transaction")]
        public Transaction TransactionData { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "redirect")]
        public string Redirect { get; set; }

        [JsonProperty(PropertyName = "checksum")]
        public string Checksum { get; set; }
    }

    public class Transaction
    {
        [JsonProperty(PropertyName = "codeId")]
        public int CodeId { get; set; }

        [JsonProperty(PropertyName = "codeMessage")]
        public string CodeMessage { get; set; }

        [JsonProperty(PropertyName = "serviceId")]
        public string ServiceId { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }
    }
}
