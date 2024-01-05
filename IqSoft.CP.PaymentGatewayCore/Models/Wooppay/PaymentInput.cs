using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Wooppay
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "account")]
        public int ClientId { get; set; }

        [JsonProperty(PropertyName = "sum")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "txn_id")]
        public string TransactionId { get; set; }
    }
}