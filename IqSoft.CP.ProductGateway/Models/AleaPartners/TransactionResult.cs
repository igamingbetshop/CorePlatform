using Newtonsoft.Json;
namespace IqSoft.CP.ProductGateway.Models.AleaPartners
{
    public class TransactionResult
    {
        [JsonProperty(PropertyName = "balance")]
        public int Balance { get; set; }
    }
}