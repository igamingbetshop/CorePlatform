using Newtonsoft.Json;
namespace IqSoft.CP.PaymentGateway.Models.Wooppay
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "account")]
        public int ClientId { get; set; }

        [JsonProperty(PropertyName = "sum")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "txn_id")]
        public string ExternalTransactionId { get; set; }

        [JsonProperty(PropertyName = "payment_txn_id")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "creation_time")]
        public string CreationTime { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "error_description")]
        public string ErrorDescription { get; set; }
    }
}