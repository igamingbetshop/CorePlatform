using Newtonsoft.Json;
namespace IqSoft.CP.ProductGateway.Models.Endorphina
{
    public class BetOutput
    {
        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public long Balance { get; set; }
    }
}