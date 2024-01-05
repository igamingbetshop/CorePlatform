using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.LuckyGaming
{
    public class LaunchH5Output
    {
        [JsonProperty(PropertyName = "errorCode")]
        public int ErrorCode { get; set; }

        [JsonProperty(PropertyName = "timeStamp")]
        public int TimeStamp { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }        
    }
}
