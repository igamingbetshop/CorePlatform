using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Omid
{
   public  class PaymentOutput
    {
        [JsonProperty(PropertyName = "error")]
        public bool Error { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "result")]
        public string Result { get; set; }
    }
}
