using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Piastrix
{
    class PayoutInput
    {
        [JsonProperty(PropertyName = "account")]
        public string Account { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "amount_type")]
        public string AmountType { get; set; }

        [JsonProperty(PropertyName = "payee_account")]
        public string PayeeAccount { get; set; }

        [JsonProperty(PropertyName = "payway")]
        public string PayWay { get; set; }

        [JsonProperty(PropertyName = "payee_currency")]
        public string PayeeCurrency { get; set; }

        [JsonProperty(PropertyName = "shop_currency")]
        public string ShopCurrency { get; set; }

        [JsonProperty(PropertyName = "shop_id")]
        public int ShopId { get; set; }

        [JsonProperty(PropertyName = "shop_payment_id")]
        public string ShopPaymentId { get; set; }

        [JsonProperty(PropertyName = "sign")]
        public string Sign { get; set; }
    }
}
