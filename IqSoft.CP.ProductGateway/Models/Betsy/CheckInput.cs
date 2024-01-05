using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Betsy
{
    public class CheckInput : BaseInput
    {
        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }
    }
}
