using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Payments.Models.NOWPay
{
    public class PaymentFiatOutput
    {
        [JsonProperty(PropertyName = "payment_id")]
        public string PaymentId { get; set; }

        [JsonProperty(PropertyName = "payment_status")]
        public string PaymentStatus { get; set; }

        [JsonProperty(PropertyName = "pay_address")]
        public string PayAddress { get; set; }

        [JsonProperty(PropertyName = "price_amount")]
        public int PriceAmount { get; set; }

        [JsonProperty(PropertyName = "price_currency")]
        public string PriceCurrency { get; set; }

        [JsonProperty(PropertyName = "pay_amount")]
        public double PayAmount { get; set; }

        [JsonProperty(PropertyName = "amount_received")]
        public double AmountReceived { get; set; }

        [JsonProperty(PropertyName = "pay_currency")]
        public string PayCurrency { get; set; }

        [JsonProperty(PropertyName = "order_id")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "order_description")]
        public object OrderDescription { get; set; }

        [JsonProperty(PropertyName = "payin_extra_id")]
        public string PayinExtraId { get; set; }

        [JsonProperty(PropertyName = "ipn_callback_url")]
        public object IpnCallbackUrl { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty(PropertyName = "updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty(PropertyName = "purchase_id")]
        public string PurchaseId { get; set; }

        [JsonProperty(PropertyName = "smart_contract")]
        public object SmartContract { get; set; }

        [JsonProperty(PropertyName = "network")]
        public string Network { get; set; }

        [JsonProperty(PropertyName = "network_precision")]
        public object NetworkPrecision { get; set; }

        [JsonProperty(PropertyName = "time_limit")]
        public object TimeLimit { get; set; }

        [JsonProperty(PropertyName = "burning_percent")]
        public object BurningPercent { get; set; }

        [JsonProperty(PropertyName = "expiration_estimate_date")]
        public DateTime ExpirationEstimateDate { get; set; }

        [JsonProperty(PropertyName = "is_fixed_rate")]
        public bool IsFixedRate { get; set; }

        [JsonProperty(PropertyName = "is_fee_paid_by_user")]
        public bool IsFeePaidByUser { get; set; }

        [JsonProperty(PropertyName = "valid_until")]
        public DateTime ValidUntil { get; set; }

        [JsonProperty(PropertyName = "redirectData")]
        public RedirectData RedirectData { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }

    public class RedirectData
    {
        [JsonProperty(PropertyName = "redirect_url")]
        public string RedirectUrl { get; set; }
    }
}