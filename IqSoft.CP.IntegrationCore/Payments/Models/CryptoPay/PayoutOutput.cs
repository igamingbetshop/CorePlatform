using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.CryptoPay
{
    public class PayoutOutput
    {
        [JsonProperty(PropertyName = "data")]
        public PayoutDetails Details { get; set; }
    }

    public class PayoutDetails
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "custom_id")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "customer_id")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "address")]
        public string WalletNumber { get; set; }

        [JsonProperty(PropertyName = "txid")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "charged_amount")]
        public string ChargedAmount { get; set; }

        [JsonProperty(PropertyName = "charged_currency")]
        public string ChargedCurrency { get; set; }

        [JsonProperty(PropertyName = "received_amount")]
        public string ReceivedAmount { get; set; }

        [JsonProperty(PropertyName = "dareceived_currencyta")]
        public string ReceivedCurrency { get; set; }

        [JsonProperty(PropertyName = "network_fee")]
        public string network_fee { get; set; }

        [JsonProperty(PropertyName = "network_fee_level")]
        public string NetworkFeeLevel { get; set; }

        [JsonProperty(PropertyName = "fee")]
        public string Fee { get; set; }

        [JsonProperty(PropertyName = "fee_currency")]
        public string FeeCurrency { get; set; }

        [JsonProperty(PropertyName = "exchange")]
        public Exchange ExchangeDetails { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public string CreatedAt { get; set; }
    }
}
