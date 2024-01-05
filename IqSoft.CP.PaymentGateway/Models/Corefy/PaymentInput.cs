using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Corefy
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "data")]
        public DataModel Data { get; set; }
    }

    public class DataModel
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "attributes")]
        public Attribute Attributes { get; set; }
    }

    public class Attribute
    {
        [JsonProperty(PropertyName = "serial_number")]
        public string SerialNumber { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "resolution")]
        public string Resolution { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "processed_amount")]
        public decimal? ProcessedAmount { get; set; }

        [JsonProperty(PropertyName = "reference_id")]
        public string ReferenceId { get; set; }

        [JsonProperty(PropertyName = "original_data")]
        public OriginalDataModel OriginalData { get; set; }

        [JsonProperty(PropertyName = "original_id")]
        public string OriginalId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }
    }

    public class OriginalDataModel
    {
        [JsonProperty(PropertyName = "original_id")]
        public string OriginalId { get; set; }
    }

    public class FlowData
    {
        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }

        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "params")]
        public object[] Parameters { get; set; }
    }
}