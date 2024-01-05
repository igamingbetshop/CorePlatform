using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.DigitalCustomer
{
    public class JCJOutput
    {
        [JsonProperty(PropertyName = "excluded")]
        public string Excluded { get; set; }

        [JsonProperty(PropertyName = "errorResponse")]
        public string ErrorResponse { get; set; }
    }
}
