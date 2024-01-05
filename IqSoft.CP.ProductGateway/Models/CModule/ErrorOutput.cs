using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.CModule
{
    public class ErrorOutput
    {
        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}