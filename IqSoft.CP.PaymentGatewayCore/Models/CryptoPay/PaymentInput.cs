using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.CryptoPay
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "event")]
        public string Event { get; set; }

        [JsonProperty(PropertyName = "data")]
        public Data Details { get; set; }
    }

    public class Data
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "txid")]
        public string TxId { get; set; }

        [JsonProperty(PropertyName = "paid_amount")]
        public decimal PaidAmount { get; set; }

        [JsonProperty(PropertyName = "paid_currency")]
        public string PaidCurrency { get; set; }

        [JsonProperty(PropertyName = "received_amount")]
        public decimal ReceivedAmount { get; set; }

        [JsonProperty(PropertyName = "received_currency")]
        public string ReceivedCurrency { get; set; }

        [JsonProperty(PropertyName = "fee")]
        public decimal Fee { get; set; }

        [JsonProperty(PropertyName = "fee_currency")]
        public string FeeCurrency { get; set; }

        [JsonProperty(PropertyName = "network_fee")]
        public decimal NetworkFee { get; set; }

        [JsonProperty(PropertyName = "exchange")]
        public Exchange ExchangeDetails { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "channel_id")]
        public string ChannelId { get; set; }

        [JsonProperty(PropertyName = "custom_id")]
        public string CustomId { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public string CreatedAt { get; set; }
    }

    public class Exchange
    {
        [JsonProperty(PropertyName = "pair")]
        public string Pair { get; set; }

        [JsonProperty(PropertyName = "rate")]
        public string Rate { get; set; }

        [JsonProperty(PropertyName = "fee")]
        public string Fee { get; set; }

        [JsonProperty(PropertyName = "fee_currency")]
        public string FeeCurrency { get; set; }
    }
}