using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.DragonGaming
{
    public class BaseInput
    {
        [JsonProperty(PropertyName = "token")]
        public string  Token { get; set; }

        [JsonProperty(PropertyName = "account_id")]
        public string  ClientId { get; set; }
    }
}