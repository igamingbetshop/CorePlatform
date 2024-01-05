using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.PremierCashier
{
    public class BaseInput
    {
        [JsonProperty(PropertyName = "frontend_id")]
        public int FrontendId { get; set; }

        [JsonProperty(PropertyName = "frontend")]
        public string Frontend { get; set; }

        [JsonProperty(PropertyName = "event")]
        public string Event { get; set; }

        [JsonProperty(PropertyName = "pin")]
        public string Pin { get; set; }

        [JsonProperty(PropertyName = "tokenname")]
        public string Tokenname { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty(PropertyName = "currency_code")]
        public string CurrencyCode { get; set; }
    }
}