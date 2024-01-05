using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.SolidGaming
{
    public class ErrorOutput
    {
        [JsonProperty(PropertyName = "responseCode")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "errorDescription")]
        public string ErrorMessage { get; set; }
    }
}