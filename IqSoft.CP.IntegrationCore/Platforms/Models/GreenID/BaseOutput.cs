using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.GreenID
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "verificationId")]
        public string VerificationId { get; set; }

        [JsonProperty(PropertyName = "verificationToken")]
        public string VerificationToken { get; set; }
    }
}