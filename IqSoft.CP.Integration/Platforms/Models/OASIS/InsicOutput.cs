using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.OASIS
{
    public class InsicOutput
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "isUserBanned")]
        public bool IsUserBanned { get; set; }
    }
}
