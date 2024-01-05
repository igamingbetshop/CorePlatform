using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.EvenBet
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "balance")]
        public long Balance { get; set; }

        [JsonProperty(PropertyName = "errorCode")]
        public int ErrorCode { get; set; }

        [JsonProperty(PropertyName = "errorDescription")]
        public string ErrorDescription { get; set; }
    }
}
