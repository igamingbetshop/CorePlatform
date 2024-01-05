﻿using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.CardPay
{
    public class PayoutOutput
    {
        [JsonProperty(PropertyName = "merchant_order")]
        public PaymentOrder MerchantOrder { get; set; }

        [JsonProperty(PropertyName = "payment_method")]
        public string PaymentMethod { get; set; }

        [JsonProperty(PropertyName = "payout_data")]
        public PayoutStatus PayoutData { get; set; }
    }

    public class PayoutStatus
    {
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "created")]
        public string CreatedDate { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "decline_code")]
        public string DeclineCode { get; set; }
    }
}
