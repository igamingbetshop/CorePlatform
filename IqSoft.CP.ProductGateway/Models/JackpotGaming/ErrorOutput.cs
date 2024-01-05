using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.JackpotGaming
{
    public class ErrorOutput
    {
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}