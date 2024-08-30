using Newtonsoft.Json;

namespace IqSoft.CP.Common.Models.AffiliateModels
{
    public class AffilkaOutput
    {
        [JsonProperty(PropertyName = "stag")]
        public string Stag { get; set; }
    }
}
