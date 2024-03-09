using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.XcoinsPay
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "fromCurrency")]
        public CurrencyModel FromCurrency { get; set; }

        [JsonProperty(PropertyName = "toCurrency")]
        public CurrencyModel ToCurrency { get; set; }

        [JsonProperty(PropertyName = "reference")]
        public string Reference { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "noMatch")]
        public bool NoMatch { get; set; }

        [JsonProperty(PropertyName = "processingFeeAmount")]
        public string ProcessingFeeAmount { get; set; }

        [JsonProperty(PropertyName = "transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty(PropertyName = "originalAmountFrom")]
        public string originalAmountFrom { get; set; }

        [JsonProperty(PropertyName = "rateExpiresAt")]
        public string RateExpiresAt { get; set; }

        [JsonProperty(PropertyName = "webhookNotificationEnabled")]
        public bool WebhookNotificationEnabled { get; set; }

        [JsonProperty(PropertyName = "merchant")]
        public Merchant Merchant { get; set; }

        [JsonProperty(PropertyName = "toWalletAddress")]
        public string ToWalletAddress { get; set; }

        [JsonProperty(PropertyName = "amlStatus")]
        public string AmlStatus { get; set; }

        [JsonProperty(PropertyName = "fromUserId")]
        public string FromUserId { get; set; }

        [JsonProperty(PropertyName = "lookUpId")]
        public string LookUpId { get; set; }

        [JsonProperty(PropertyName = "rate")]
        public string Rate { get; set; }

        [JsonProperty(PropertyName = "networkFee")]
        public string NetworkFee { get; set; }

        [JsonProperty(PropertyName = "amountTo")]
        public string AmountTo { get; set; }

        [JsonProperty(PropertyName = "amountFrom")]
        public string AmountFrom { get; set; }

        [JsonProperty(PropertyName = "settlementCurrency")]
        public string SettlementCurrency { get; set; }

        [JsonProperty(PropertyName = "externalFee")]
        public string ExternalFee { get; set; }

        [JsonProperty(PropertyName = "walletDeclarationRequired")]
        public bool WalletDeclarationRequired { get; set; }
    }

    public class CurrencyModel : Merchant
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }

    public class Merchant
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}