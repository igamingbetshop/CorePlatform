using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.OutcomeBet
{
    public class ResponseBase
    {
        [JsonProperty(PropertyName = "jsonrpc")]
        public string JsonRpc { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string RequestId { get; set; }

        [JsonProperty(PropertyName = "error")]
        public ErrorOutput Error { get; set; }

        [JsonProperty(PropertyName = "result")]
        public object Result { get; set; }
    }

    public class ErrorOutput
    {
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }
    }
}
