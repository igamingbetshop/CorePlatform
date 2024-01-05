using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.MGTCompliance
{
    public class RegisterOutput : ErrorOutput
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "identUrl")]
        public string IdentUrl { get; set; }
    }
}
