using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.OutcomeBet
{
    public class BaseInput
    {
        [JsonProperty(PropertyName = "jsonrpc")]
        public string JsonRpc { get; set; }

        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "id")]
        public long RequestId { get; set; }

        [JsonProperty(PropertyName = "params")]
        public object Params { get; set; }
    }

    public class ContextModel
    {
        public string GameId { get; set; }
        public string SessionId { get; set; }
        public string SessionAlternativeId { get; set; }
        public string BetType { get; set; }
        public string WinType { get; set; }
    }
}