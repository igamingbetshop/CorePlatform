using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.GMW
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "playerid")]
        public string PlayerId { get; set; }

        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public long Balance { get; set; }

        [JsonProperty(PropertyName = "currencycode")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "errorcode")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "errordescription")]
        public string ErrordDescription { get; set; }
    }
}