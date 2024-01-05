using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Elite
{
    public class CurrentBalance
    {
        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "cashBalance")]
        public decimal CashBalance { get; set; }

        [JsonProperty(PropertyName = "bonusBalance")]
        public decimal BonusBalance { get; set; } = 0;

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "conversionRate")]
        public string ConversionRate { get; set; }
    }

    public class Error
    {
        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "currentBalance")]
        public CurrentBalance CurrentBalance { get; set; }
    }

    public class ErrorOutput
    {
        [JsonProperty(PropertyName = "error")]
        public Error Error { get; set; }
    }
}