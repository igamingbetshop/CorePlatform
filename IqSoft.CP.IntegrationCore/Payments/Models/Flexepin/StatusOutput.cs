using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Flexepin
{
    public class StatusOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
}
