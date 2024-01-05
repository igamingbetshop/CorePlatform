using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Singular
{
    public class BaseInput
    {
        [JsonProperty(PropertyName = "providerID")]
        public string OperatorId { get; set; }

        [JsonProperty(PropertyName = "hash")]
        public string Hash { get; set; }
    }
}