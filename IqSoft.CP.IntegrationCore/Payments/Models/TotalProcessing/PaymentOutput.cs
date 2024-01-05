using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.TotalProcessing
{
   public class PaymentOutput
    {
        [JsonProperty("result")]
        public OutputResult Result { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

    }

    public class OutputResult
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
