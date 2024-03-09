using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Changelly
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "redirectUrl")]
        public string RedirectUrl { get; set; }

        [JsonProperty(PropertyName = "orderId")]
        public string OrderId { get; set; }

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

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "walletAddress")]
        public string WalletAddress { get; set; }

        [JsonProperty(PropertyName = "walletExtraId")]
        public string WalletExtraId { get; set; }

        [JsonProperty(PropertyName = "paymentMethod")]
        public string PaymentMethod { get; set; }
    }
}
