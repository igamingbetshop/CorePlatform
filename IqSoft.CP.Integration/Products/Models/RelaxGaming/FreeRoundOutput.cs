using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.RelaxGaming
{
    public class FreeRoundOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
}
