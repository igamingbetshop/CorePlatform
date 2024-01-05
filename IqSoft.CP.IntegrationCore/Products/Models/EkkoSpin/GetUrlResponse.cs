using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.EkkoSpin
{
    public class GetUrlResponse
    {
        [JsonProperty(PropertyName = "done")]
        public int Done { get; set; }
        [JsonProperty(PropertyName = "errors")]
        public string[] Errors { get; set; }
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}