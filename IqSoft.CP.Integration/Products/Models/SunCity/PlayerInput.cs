using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.SunCity
{
    public class PlayerInput
    {
        [JsonProperty(PropertyName = "ipaddress")]
        public string IpAddress { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "userid")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "tag")]
        public string Tag { get; set; }

        [JsonProperty(PropertyName = "lang")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "cur")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "betlimitid")]
        public int BetLimitId { get; set; }

        [JsonProperty(PropertyName = "istestplayer")]
        public bool IsTestPlayer { get; set; }

        [JsonProperty(PropertyName = "platformtype")]
        public int PlatformType { get; set; }
    }
}
