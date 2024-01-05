using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.ISoftBet
{
    public class TokenOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }
    }
}