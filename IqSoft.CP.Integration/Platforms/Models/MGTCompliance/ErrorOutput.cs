using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.MGTCompliance
{
    public class ErrorOutput
    {
        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}
