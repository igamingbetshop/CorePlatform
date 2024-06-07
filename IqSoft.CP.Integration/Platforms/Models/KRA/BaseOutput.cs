using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.KRA
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "RESPONSE")]
        public ResponseModel Response { get; set; }
    }

    public class ResponseModel
    {
        [JsonProperty(PropertyName = "RESPONSE")]
        public RESULT RESULT { get; set; }
    }

    public class RESULT
    {
        public string ResponseCode { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
    }
}