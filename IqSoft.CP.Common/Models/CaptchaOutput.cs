using Newtonsoft.Json;

namespace IqSoft.CP.Common.Models
{
    public class CaptchaOutput
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "challenge_ts")]
        public string ChallengeTs { get; set; }

        [JsonProperty(PropertyName = "hostname")]
        public string Hostname { get; set; }

        [JsonProperty(PropertyName = "error-codes")]
        public string Error { get; set; }
    }
}
