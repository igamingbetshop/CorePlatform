using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.OutcomeBet
{
  public  class RequestBase
    {
        [JsonProperty(PropertyName = "jsonrpc")]
        public string JsonRpc { get; set; }

        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string RequestId { get; set; }

        [JsonProperty(PropertyName = "params")]
        public object Params { get; set; }
    }
}
