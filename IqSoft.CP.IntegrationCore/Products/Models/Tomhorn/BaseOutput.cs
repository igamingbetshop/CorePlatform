using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.Tomhorn
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "Code")]
        public int Code { get; set; }

        [JsonProperty(PropertyName = "Message")]
        public string MessageText { get; set; }
    }
}
