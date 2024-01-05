using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.SolidGaming
{
    public class BaseInput
    {
        //[JsonProperty(PropertyName = "sessionid")]
        //public string SessionId { get; set; }

        [JsonProperty(PropertyName = "gamecode")]
        public string GameCode { get; set; }

        //[JsonProperty(PropertyName = "platform")]
        //public string Platform { get; set; }
    }
}