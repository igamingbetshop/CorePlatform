using Newtonsoft.Json;

namespace IqSoft.CP.Common.Models
{
    public class ItemProperty
    {
        [JsonProperty(PropertyName = "mandatory")]
        public string Mandatory { get; set; }

        [JsonProperty(PropertyName = "regExp")]
        public string Regex { get; set; }
    }
}
