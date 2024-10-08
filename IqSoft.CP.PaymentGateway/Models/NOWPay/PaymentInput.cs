using Newtonsoft.Json;
using System;

namespace IqSoft.CP.PaymentGateway.Models.NOWPay
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "payment_id")]
        public string PaymentId { get; set; }

        [JsonProperty(PropertyName = "payment_status")]
        public string PaymentStatus { get; set; }

        [JsonProperty(PropertyName = "pay_address")]
        public string PayAddress { get; set; }

        [JsonProperty(PropertyName = "price_amount")]
        public decimal PriceAmount { get; set; }

        [JsonProperty(PropertyName = "price_currency")]
        public string PriceCurrency { get; set; }

        [JsonProperty(PropertyName = "pay_amount")]
        public decimal PayAmount { get; set; }

        [JsonProperty(PropertyName = "pay_currency")]
        public string PayCurrency { get; set; }

        [JsonProperty(PropertyName = "order_id")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "order_description")]
        public string OrderDescription { get; set; }

        [JsonProperty(PropertyName = "ipn_callback_url")]
        public string IpnCallbackUrl { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty(PropertyName = "updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty(PropertyName = "purchase_id")]
        public string Purchase_id { get; set; }

        [JsonProperty(PropertyName = "outcome_amount")]
        public decimal Outcome_amount { get; set; }

        [JsonProperty(PropertyName = "actually_paid")]
        public decimal Actually_paid { get; set; }
    }
}
