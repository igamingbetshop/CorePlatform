using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Changelly
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "redirectUrl")]
        public string RedirectUrl { get; set; }

        [JsonProperty(PropertyName = "orderId")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "externalUserId")]
        public string ExternalUserId { get; set; }

        [JsonProperty(PropertyName = "externalOrderId")]
        public string ExternalOrderId { get; set; }

        [JsonProperty(PropertyName = "providerCode")]
        public string ProviderCode { get; set; }

        [JsonProperty(PropertyName = "currencyFrom")]
        public string CurrencyFrom { get; set; }

        [JsonProperty(PropertyName = "currencyTo")]
        public string CurrencyTo { get; set; }

        [JsonProperty(PropertyName = "amountFrom")]
        public string AmountFrom { get; set; }

        [JsonProperty(PropertyName = "walletAddress")]
        public string WalletAddress { get; set; }

        [JsonProperty(PropertyName = "walletExtraId")]
        public string WalletExtraId { get; set; }

        [JsonProperty(PropertyName = "paymentMethod")]
        public string PaymentMethod { get; set; }

        [JsonProperty(PropertyName = "payinAmount")]
        public string PayinAmount { get; set; }

        [JsonProperty(PropertyName = "payoutAmount")]
        public string PayoutAmount { get; set; }

        [JsonProperty(PropertyName = "payinCurrency")]
        public string PayinCurrency { get; set; }

        [JsonProperty(PropertyName = "payoutCurrency")]
        public string PayoutCurrency { get; set; }
    }
}