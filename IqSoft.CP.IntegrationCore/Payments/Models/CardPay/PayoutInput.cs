using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.CardPay
{
    public class PayoutInput
    {
        [JsonProperty(PropertyName = "ewallet_account")]
        public EWalletAccount Account { get; set; }

        [JsonProperty(PropertyName = "merchant_order")]
        public PaymentOrder MerchantOrder { get; set; }

        [JsonProperty(PropertyName = "payment_method")]
        public string PaymentMethod { get; set; }

        [JsonProperty(PropertyName = "payout_data")]
        public PaymentData PayoutData { get; set; }

        [JsonProperty(PropertyName = "request")]
        public PaymentRequestDetails Request { get; set; }

        [JsonProperty(PropertyName = "card_account")]
        public CardAccount CardAccount { get; set; }
    }

    public class EWalletAccount
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "bank_branch")]
        public string BankBranch { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }
}
