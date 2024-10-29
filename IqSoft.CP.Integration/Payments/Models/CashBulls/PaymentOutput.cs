using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.CashBulls
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "status_transaction")]
        public string StatusTransaction { get; set; }

        [JsonProperty(PropertyName = "order_id")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "payment_id")]
        public string PaymentId { get; set; }

        [JsonProperty(PropertyName = "redirect_url")]
        public string RedirectUrl { get; set; }
    }
}
