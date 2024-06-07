using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.FreedomPay
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "code")]
        public string code { get; set; }

        [JsonProperty(PropertyName = "order_id")]
        public string order_id { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string currency { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal amount { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string state { get; set; }

        [JsonProperty(PropertyName = "card_brand")]
        public string card_brand { get; set; }

        [JsonProperty(PropertyName = "card_number")]
        public string card_number { get; set; }

        [JsonProperty(PropertyName = "expiry")]
        public ExpiryModel expiry { get; set; }

        [JsonProperty(PropertyName = "failure")]
        public string failure { get; set; }
    }

    public class ExpiryModel
    {
        [JsonProperty(PropertyName = "year")]
        public string year { get; set; }

        [JsonProperty(PropertyName = "month")]
        public string month { get; set; }
    }
}