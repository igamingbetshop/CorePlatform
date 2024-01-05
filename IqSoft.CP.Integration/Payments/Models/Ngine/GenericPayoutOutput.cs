using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Ngine
{
    public class PayoutAuthentication
    {
        [JsonProperty(PropertyName = "TransactionID")]
        public string TransactionID { get; set; }

        [JsonProperty(PropertyName = "ErrorDescription")]
        public object ErrorDescription { get; set; }

        [JsonProperty(PropertyName = "ErrorCode")]
        public object ErrorCode { get; set; }

        [JsonProperty(PropertyName = "Status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "Amount")]
        public double Amount { get; set; }

        [JsonProperty(PropertyName = "CurrencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty(PropertyName = "HtmlResponse")]
        public string HtmlResponse { get; set; }
    }

    public class GenericPayoutOutput
    {
        [JsonProperty(PropertyName = "Authentication")]
        public PayoutAuthentication Authentication { get; set; }
    }
}
