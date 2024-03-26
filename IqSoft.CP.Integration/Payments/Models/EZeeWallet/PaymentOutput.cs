using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.EZeeWallet
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "transfer")]
        public Transfer TransferDetails { get; set; }

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
}