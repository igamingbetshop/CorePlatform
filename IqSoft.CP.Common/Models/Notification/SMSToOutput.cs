using Newtonsoft.Json;

namespace IqSoft.CP.Common.Models.Notification
{
    public class SMSToOutput
    {
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }
    }
}
