using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.KRA
{
    public class AuthOutput
    {
        [JsonProperty(PropertyName = "id_token")]
        public string IdToken { get; set; }
    }
}
