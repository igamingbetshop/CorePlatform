using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.VisionaryiGaming
{
    public class AuthenticationInput
    {
        [JsonProperty(PropertyName = "ViGuser")]
        public string ViGuser { get; set; }

        [JsonProperty(PropertyName = "OTP")]
        public string OTP { get; set; }

        [JsonProperty(PropertyName = "siteID")]
        public string SiteID { get; set; }
    }
}