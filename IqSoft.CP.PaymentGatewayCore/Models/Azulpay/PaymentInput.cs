using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Azulpay
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "order_id")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "customer_order_id")]
        public string CustomerOrderId { get; set; }

        [JsonProperty(PropertyName = "transaction_status")]
        public string TransactionStatus { get; set; }

        [JsonProperty(PropertyName = "reason")]
        public string Reason { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "transaction_date")]
        public string TransactionDate { get; set; }
    }
}