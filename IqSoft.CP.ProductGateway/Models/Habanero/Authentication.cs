using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Habanero
{
    public class Authentication
    {
        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "passkey")]
        public string PassKey { get; set; }

        [JsonProperty(PropertyName = "machinename")]
        public string MachineName { get; set; }

        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }

        [JsonProperty(PropertyName = "brandid")]
        public string Brandid { get; set; }
    }

    public class Player
    {
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "gamelaunch")]
        public bool Gamelaunch { get; set; }
    }
}