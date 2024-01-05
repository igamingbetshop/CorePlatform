using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.BRPay
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "sp_result")]
        public int Result { get; set; }

        [JsonProperty(PropertyName = "sp_order_id")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "sp_payment_id")]
        public string PaymentId { get; set; }

        [JsonProperty(PropertyName = "sp_payment_system")]
        public string PaymentSystem { get; set; }

        [JsonProperty(PropertyName = "sp_amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "sp_ps_amount")]
        public decimal PsAmount { get; set; }

        [JsonProperty(PropertyName = "sp_net_amount")]
        public decimal NetAmount { get; set; }

        [JsonProperty(PropertyName = "sp_currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "sp_salt")]
        public string Salt { get; set; }

        [JsonProperty(PropertyName = "sp_sig")]
        public string Signature { get; set; }
    }
}