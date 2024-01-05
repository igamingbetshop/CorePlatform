using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Endorphina
{
    public class ErrorOutput
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}