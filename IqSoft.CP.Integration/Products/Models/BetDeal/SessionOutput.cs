using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.BetDeal
{
    public class SessionOutput
    {
        [JsonProperty( "data")]
        public Data data { get; set; }
    }

    public class Data
    {
        [JsonProperty("type")]
        public string type { get; set; }

        [JsonPropertyAttribute("attributes")]
        public Attributes attributes { get; set; }
    }

    public class Attributes
    {
        [JsonProperty("redirect-url")]
        public string @RedirectUrl { get; set; }

        [JsonProperty("session-id")]
        public string SessionId  { get; set; }
    }
}
