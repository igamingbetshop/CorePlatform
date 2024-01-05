using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.MaldoPay
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "result")]
        public string Result { get; set; }

        [JsonProperty(PropertyName = "codeId")]
        public int CodeId { get; set; }

        [JsonProperty(PropertyName = "serviceId")]
        public string ServiceId { get; set; }

        [JsonProperty(PropertyName = "dateUpdated")]
        public string DateUpdated { get; set; }

        [JsonProperty(PropertyName = "referenceOrderId")]
        public string ReferenceOrderId { get; set; }

        [JsonProperty(PropertyName = "maldoTransactionId")]
        public string ExternalTransactionId { get; set; }

        [JsonProperty(PropertyName = "reason")]
        public string Reason { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty(PropertyName = "conversion")]
        public Conversion Conversion { get; set; }

        [JsonProperty(PropertyName = "checksum")]
        public string Checksum { get; set; }
    }

    public class Conversion
    {
        [JsonProperty(PropertyName = "converted_amount")]
        public decimal ConvertedAmount { get; set; }

        [JsonProperty(PropertyName = "converted_rate")]
        public string ConvertedRate { get; set; }

        [JsonProperty(PropertyName = "converted_to")]
        public string ConvertedTo { get; set; }
    }
}