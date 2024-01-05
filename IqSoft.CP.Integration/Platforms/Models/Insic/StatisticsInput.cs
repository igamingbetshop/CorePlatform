using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.Insic
{
    public class StatisticsInput : RequestBase
    {
        [JsonProperty(PropertyName = "stake")]
        public decimal Stake { get; set; }

        [JsonProperty(PropertyName = "profit")]
        public decimal Profit { get; set; }

        [JsonProperty(PropertyName = "loss")]
        public decimal Loss { get; set; }
    }
}
