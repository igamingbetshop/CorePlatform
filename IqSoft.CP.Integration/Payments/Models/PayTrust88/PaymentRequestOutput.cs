using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.PayTrust88
{
    class PaymentRequestOutput
    {
        [JsonProperty(PropertyName = "transaction")]
        public long transaction { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int status { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string token { get; set; }

        [JsonProperty(PropertyName = "redirect_to")]
        public string redirect_to { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string currency { get; set; }
    }
}