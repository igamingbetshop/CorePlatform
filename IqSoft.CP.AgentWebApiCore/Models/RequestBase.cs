using Newtonsoft.Json;

namespace IqSoft.CP.AgentWebApi.Models
{
    public class RequestBase
    {
        public string Controller { get; set; }
        public string Method { get; set; }
        public string Token { get; set; }
        public string SecurityCode { get; set; }
        public string RequestData { get { return JsonConvert.SerializeObject(RequestObject); } }
        public object RequestObject { get; set; }
    }
}