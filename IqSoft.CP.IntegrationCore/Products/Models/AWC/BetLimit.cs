using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.AWC
{
   public class BetLimit
    {
        [JsonProperty(PropertyName = "SEXYBCRT")]
        public PlatformType Platform { get; set; }
    }

    public class PlatformType
    {
        [JsonProperty(PropertyName = "LIVE")]
        public LimitType Type { get; set; }
    }

    public class LimitType
    {
        [JsonProperty(PropertyName = "limitId")]
        public List<int> LimitId { get; set; }
    }
}
