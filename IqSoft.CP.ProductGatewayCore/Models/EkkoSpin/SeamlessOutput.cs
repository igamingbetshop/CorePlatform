using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.EkkoSpin
{
    public class SeamlessOutput
    {
        [JsonProperty(PropertyName ="done")]
        public int Success { get; set; }

        [JsonProperty(PropertyName = "id_stat")]
        public long TransactionId { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }
    }
}