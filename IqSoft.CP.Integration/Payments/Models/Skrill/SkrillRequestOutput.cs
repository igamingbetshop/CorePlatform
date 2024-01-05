using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Skrill
{
    public class SkrillRequestOutput
    {
        [JsonProperty(PropertyName = "message")]
        public string ErrorMessage { get; set; }

        [JsonProperty(PropertyName = "code")]
        public int ErroreCode { get; set; }
    }
}