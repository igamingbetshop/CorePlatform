using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.OutcomeBet
{
    public class BaseResponse
    {
        [JsonProperty(PropertyName = "jsonrpc")]
        public string JsonRpc { get; set; }

        [JsonProperty(PropertyName = "id")]
        public long RequestId { get; set; }

        [JsonProperty(PropertyName = "result")]
        public object Result { get; set; }

        [JsonProperty(PropertyName = "error")]
        public Error ErrorData { get; set; }
    }

    public class Error
    {
        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}