using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.CardPay
{
    public class PaymentRequestInput
    {
        //[JsonProperty(PropertyName = "card_account")]
        //public CardAccount CardAccount { get; set; }

        [JsonProperty(PropertyName = "customer")]
        public PaymentCustomer MerchantCustomer { get; set; }

        [JsonProperty(PropertyName = "merchant_order")]
        public PaymentOrder MerchantOrder { get; set; }

        [JsonProperty(PropertyName = "payment_data")]
        public PaymentData PaymentDataDetails { get; set; }

        [JsonProperty(PropertyName = "payment_method")]
        public string PaymentMethod { get; set; }

        [JsonProperty(PropertyName = "request")]
        public PaymentRequestDetails RequestDetails { get; set; }

        [JsonProperty(PropertyName = "returnUrls")]
        public ReturnUrls ReturnUrl { get; set; }

    }

    public class PaymentCustomer
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "phone")]
        public string MobileNumber { get; set; }
    }

    public class PaymentOrder
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }

    public class CardObject
    {
        [JsonProperty(PropertyName = "pan")]
        public string Pan { get; set; }

        [JsonProperty(PropertyName = "expiration")]
        public string ExpireDate { get; set; }
    }

        public class CardAccount
    {
        [JsonProperty(PropertyName = "card")]
        public CardObject Card { get; set; }

        [JsonProperty(PropertyName = "recipient_info")]
        public string RecipientInfo { get; set; }
    }

    public class PaymentData
    {
        [JsonProperty(PropertyName = "amount")]
        public int Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }
    }

    public class PaymentRequestDetails
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "time")]
        public string RequestTime { get; set; }
    }

    public class ReturnUrls
    {
        [JsonProperty(PropertyName = "cancel_url")]
        public string CancelUrl { get; set; }

        [JsonProperty(PropertyName = "decline_url")]
        public string DeclineUrl { get; set; }

        [JsonProperty(PropertyName = "inprocess_url")]
        public string InprocessUrl { get; set; }

        [JsonProperty(PropertyName = "return_url")]
        public string ReturnUrl { get; set; }

        [JsonProperty(PropertyName = "success_url")]
        public string SuccessUrl { get; set; }
    }
}
