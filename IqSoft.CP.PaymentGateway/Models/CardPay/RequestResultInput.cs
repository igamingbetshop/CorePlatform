using IqSoft.CP.Common.Helpers;
using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.CardPay
{
    public class RequestResultInput
    {
        [JsonProperty(PropertyName = "callback_time")]
        public string RequestTime { get; set; }

        [JsonProperty(PropertyName = "payment_method")]
        public string PaymentMethod { get; set; }

        [JsonProperty(PropertyName = "merchant_order")]
        public PaymentOrder Order { get; set; }

        [JsonProperty(PropertyName = "customer")]
        public CustomerData Customer { get; set; }

        [JsonProperty(PropertyName = "payment_data")]
        public PaymentData PaymentDetails { get; set; }

        [JsonProperty(PropertyName = "payout_data")]
        public PayoutData PayoutDetails { get; set; }

        [JsonProperty(PropertyName = "card_account")]
        public CardAccountData CardAccount { get; set; }

        [JsonProperty(PropertyName = "ewallet_account")]
        public Wallet WalletAccount { get; set; }
    }

    public class PaymentOrder
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }

    public class PaymentData
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "amount")]
        [JsonConverter(typeof(DecimalJsonConverter))]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "created")]
        public string CreatedTime { get; set; }

        [JsonProperty(PropertyName = "rrn")]
        public string RRN { get; set; }

        [JsonProperty(PropertyName = "auth_code")]
        public string AuthCode { get; set; }

        [JsonProperty(PropertyName = "is_3d")]
        public bool? Is3D { get; set; }

    }

    public class CardAccountData
    {
        [JsonProperty(PropertyName = "masked_pan")]
        public string MaskedPan { get; set; }

        [JsonProperty(PropertyName = "issuing_country_code")]
        public string CountryCode { get; set; }

        [JsonProperty(PropertyName = "holder")]
        public string Holder { get; set; }

        [JsonProperty(PropertyName = "expiration")]
        public string Expiration { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }
    }

    public class CustomerData
    {
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "ip")]
        public string Ip { get; set; }

        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }
    }

    public class PayoutData
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "amount")]
        [JsonConverter(typeof(DecimalJsonConverter))]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "created")]
        public string CreatedTime { get; set; }
                                 
        [JsonProperty(PropertyName = "note")]
        public string Note { get; set; }

        [JsonProperty(PropertyName = "rrn")]
        public string RRN { get; set; }
        
        [JsonProperty(PropertyName = "decline_reason")]
        public string DeclineReason { get; set; }
    }

    public class Wallet
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }   
}