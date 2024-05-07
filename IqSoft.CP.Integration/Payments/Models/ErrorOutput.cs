using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models
{
    public class ErrorOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}
