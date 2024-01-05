using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.SkyCity
{
    public class BaseInput
    {
        [JsonProperty(PropertyName = "userid")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "gamecode")]
        public int GameCode { get; set; }

        [JsonProperty(PropertyName = "rtn")]
        public string Description { get; set; }
    }
}