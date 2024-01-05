using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.Evolution
{
    public class AuthenticationOutput
    {
        [JsonProperty(PropertyName = "entry")]
        public string Entry { get; set; }

        [JsonProperty(PropertyName = "entryEmbedded")]
        public string EntryEmbedded { get; set; }
    }
}
