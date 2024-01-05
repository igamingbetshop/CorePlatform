using Newtonsoft.Json;

namespace IqSoft.CP.AdminWebApi.Models.AdminModels
{
    public class ApiRequestInput
    {
        public string ApiKey { get; set; }
        public int UserId { get; set; }
        public string Method { get; set; }
        public string LanguageId { get; set; }
        public double TimeZone { get; set; }
        public string RequestData { get { return JsonConvert.SerializeObject(RequestObject); } }
        public object RequestObject { get; set; }
    }
}