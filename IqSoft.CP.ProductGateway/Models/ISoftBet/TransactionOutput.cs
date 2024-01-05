using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.ISoftBet
{
    public class TransactionOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "balance")]
        public long Balance { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }
    }
}