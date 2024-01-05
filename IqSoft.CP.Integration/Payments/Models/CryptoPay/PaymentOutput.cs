using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.CryptoPay
{
   /* public class PaymentOutput
    {
        [JsonProperty(PropertyName = "customer_id")]
        public string CustomerId { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "receiver_currency")]
        public string ReceiverCurrency { get; set; }

        [JsonProperty(PropertyName = "pay_currency")]
        public string PayCurrency { get; set; }

        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "project_id")]
        public string ProjectId { get; set; }

        [JsonProperty(PropertyName = "custom_id")]
        public string CustomId { get; set; }

        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; }

        [JsonProperty(PropertyName = "hosted_page_url")]
        public string HostedPageUrl { get; set; }
    }*/

    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "data")]
        public DataDetails Details { get; set; }
    }

    public class DataDetails
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "status_context")]
        public string StatusContext { get; set; }

        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; }

        [JsonProperty(PropertyName = "price_amount")]
        public string PriceAmount { get; set; }

        [JsonProperty(PropertyName = "price_currency")]
        public string PriceCurrency { get; set; }

        [JsonProperty(PropertyName = "pay_amount")]
        public string PayAmount { get; set; }

        [JsonProperty(PropertyName = "pay_currency")]
        public string PayCurrency { get; set; }

        [JsonProperty(PropertyName = "paid_amount")]
        public string PaidAmount { get; set; }

        [JsonProperty(PropertyName = "exchange")]
        public Exchange ExchangeDetails { get; set; }

        [JsonProperty(PropertyName = "success_redirect_url")]
        public string SuccessRedirectUrl { get; set; }

        [JsonProperty(PropertyName = "unsuccess_redirect_url")]
        public string UnsuccessRedirectUrl { get; set; }

        [JsonProperty(PropertyName = "hosted_page_url")]
        public string HostedPageUrl { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty(PropertyName = "expires_at")]
        public string ExpiresAt { get; set; }
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