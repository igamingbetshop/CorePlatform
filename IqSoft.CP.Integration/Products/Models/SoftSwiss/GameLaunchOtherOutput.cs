using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.SoftSwiss.GameLaunchOther
{
    public class GameLaunchOtherOutput
    {
        [JsonProperty(PropertyName = "launch_options")]
        public LaunchOptions LaunchParameters { get; set; }
    }

    public class LaunchOptions
    {
        [JsonProperty(PropertyName = "desktop_url")]
        public string DesktopUrl { get; set; }

        [JsonProperty(PropertyName = "mobile_url")]
        public string MobileUrl { get; set; }

        [JsonProperty(PropertyName = "return_url")]
        public string ReturnUrl { get; set; }

        [JsonProperty(PropertyName = "origin")]
        public string Origin { get; set; }

        [JsonProperty(PropertyName = "strategy")]
        public string Strategy { get; set; }

        [JsonProperty(PropertyName = "client_type")]
        public string ClientType { get; set; }
    }
}
