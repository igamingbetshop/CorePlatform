using IqSoft.CP.ProductGateway.Models.ISoftBet;
using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.SolidGaming
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty(PropertyName = "balance")]
        [JsonConverter(typeof(DecimalJsonConverter))]
        public decimal Balance { get; set; }
    }
}