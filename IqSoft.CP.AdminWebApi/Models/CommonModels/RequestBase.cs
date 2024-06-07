using Newtonsoft.Json;

namespace IqSoft.CP.AdminWebApi.Models.CommonModels
{
    public class RequestBase
    {
        public string Controller { get; set; }
        public string Method { get; set; }
        public string Token { get; set; }
        public int? UserId { get; set; }
        public int? ClientId { get; set; }
        public string ApiKey { get; set; }
        public string RequestData { get { return JsonConvert.SerializeObject(RequestObject); } }
        public object RequestObject { get; set; }
    }
}