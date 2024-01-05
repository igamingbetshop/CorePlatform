using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.DigitalCustomer
{
    public class AMLOutput
    {
        [JsonProperty(PropertyName = "verified")]
        public string Verified { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "percentage")]
        public decimal? Percentage { get; set; }

        [JsonProperty(PropertyName = "errorDescription")]
        public string ErrorDescription { get; set; }
    }
}