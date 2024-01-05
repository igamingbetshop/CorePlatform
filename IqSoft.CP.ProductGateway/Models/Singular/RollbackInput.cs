using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Singular
{
    public class RollbackInput : BaseInput
    {
        [JsonProperty(PropertyName = "transactionOfProviderID")]
        public string TransactionOfProviderId { get; set; }     //Always NULL

        [JsonProperty(PropertyName = "transactionID")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "isCoreTransactionID")]
        public bool IsCoreTransactionId { get; set; }

        [JsonProperty(PropertyName = "statusNote")]
        public string StatusNote { get; set; }      //optional
    }
}