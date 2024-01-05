using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Ngine
{
    public class DepositAuthentication
    {
        [JsonProperty(PropertyName = "TransactionID")]
        public string TransactionID { get; set; }

        [JsonProperty(PropertyName = "CashierID")]
        public int CashierID { get; set; }

        [JsonProperty(PropertyName = "ErrorDescription")]
        public string ErrorDescription { get; set; }

        [JsonProperty(PropertyName = "ErrorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "TraceID")]
        public string TraceID { get; set; }

        [JsonProperty(PropertyName = "externalID")]
        public string ExternalID { get; set; }

        [JsonProperty(PropertyName = "Amount")]
        public double Amount { get; set; }

        [JsonProperty(PropertyName = "CurrencyCode")]
        public object CurrencyCode { get; set; }

        [JsonProperty(PropertyName = "RejectReason")]
        public object RejectReason { get; set; }

        [JsonProperty(PropertyName = "Notes")]
        public string Notes { get; set; }

        [JsonProperty(PropertyName = "CSID")]
        public int CSID { get; set; }

        [JsonProperty(PropertyName = "HtmlResponse")]
        public string HtmlResponse { get; set; }

        [JsonProperty(PropertyName = "ProcessorName")]
        public string ProcessorName { get; set; }

        [JsonProperty(PropertyName = "Descriptor")]
        public string Descriptor { get; set; }
    }

    public class CreditCardDepositOutput
    {
        [JsonProperty(PropertyName = "Authentication")]
        public DepositAuthentication Authentication { get; set; }
    }
}