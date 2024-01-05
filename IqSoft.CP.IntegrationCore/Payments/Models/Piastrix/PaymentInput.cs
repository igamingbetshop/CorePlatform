using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Piastrix
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "amount")]
        public string amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public  string currency { get; set; }

        [JsonProperty(PropertyName = "shop_id")]
        public string shop_id { get; set; }

        [JsonProperty(PropertyName = "shop_order_id")]
        public long shop_order_id { get; set; }

        [JsonProperty(PropertyName = "sign")]
        public string sign { get; set; }

        [JsonProperty(PropertyName = "payway")]
        public string payway { get; set; }

        [JsonProperty(PropertyName = "phone")]
        public string phone { get; set; }

        [JsonProperty(PropertyName = "success_url")]
        public string success_url { get; set; }

        [JsonProperty(PropertyName = "failed_url")]
        public string failed_url { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string description { get; set; }

        [JsonProperty(PropertyName = "payer_id")]
        public string payer_id { get; set; }
    }
}
