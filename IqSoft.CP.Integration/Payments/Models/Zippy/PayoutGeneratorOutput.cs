using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Zippy
{
   public class PayoutGeneratorOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }
}
