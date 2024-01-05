using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.Evenbet
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "data")]
        public Data MainData { get; set; }

        [JsonProperty(PropertyName = "error")]
        public object[] Error { get; set; }
    }

    public class Data
    {
        [JsonProperty(PropertyName = "attributes")]
        public Attribute Attributes { get; set; }
    }
    public class Attribute
    {
        [JsonProperty(PropertyName = "redirect-url")]
        public string RedirectUrl { get; set; }
    }
}