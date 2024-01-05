using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.EZeeWallet
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "transfer")]
        public Transfer TransferDetails { get; set; }

        [JsonProperty(PropertyName = "error")]
        public ErrorModel Error { get; set; }
    }

    public class Transfer
    {
        [JsonProperty(PropertyName = "transaction_id")]
        public string MerchantTransactionId { get; set; }

        [JsonProperty(PropertyName = "unique_id")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }

    public class ErrorModel
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}