using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Pay4Fun
{
    public class PaymentOutput
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        
        [JsonProperty("message")]
        public string Message  { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
