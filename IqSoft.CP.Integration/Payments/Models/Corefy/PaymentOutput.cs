using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Corefy
{
    public class PaymentOutput
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

        [JsonProperty(PropertyName = "flow_data")]
        public object FlowData { get; set; }    

        [JsonProperty(PropertyName = "hpp_url")]
        public string HppUrl { get; set; }        
    }

    public class FlowData
    {
        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }

        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

    }
}