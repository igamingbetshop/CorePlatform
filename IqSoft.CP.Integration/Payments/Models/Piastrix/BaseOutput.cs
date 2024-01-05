using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Piastrix
{
    public class BaseOutput
    {     
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "error_code")]
        public int error_code { get; set; }

        [JsonProperty(PropertyName = "result")]
        public bool Result { get; set; }
    }   
}
