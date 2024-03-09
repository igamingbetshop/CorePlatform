using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Flexepin
{
    public class BaseOutput 
    {
        [JsonProperty(PropertyName = "result")]
        public int Result { get; set; }

        [JsonProperty(PropertyName = "result_description")]
        public string ResultDescription { get; set; }
    }
}