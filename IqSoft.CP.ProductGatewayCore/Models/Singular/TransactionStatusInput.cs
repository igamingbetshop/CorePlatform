using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Singular
{
    public class TransactionStatusInput : BaseInput
    {
        [JsonProperty(PropertyName = "transactionID")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "isCoreTransactionID")]
        public bool IsCoreTransactionId { get; set; }
    }
}