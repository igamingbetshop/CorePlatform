using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.TotalProcessing
{
    public class ResultInput
    {
        [JsonProperty("result")]
        public OutputResult Result { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

    }

    public class OutputResult
    {
        public string Code { get; set; }

        public string Description { get; set; }
    }
}