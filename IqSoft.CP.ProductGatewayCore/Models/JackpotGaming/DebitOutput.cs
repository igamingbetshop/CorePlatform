using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.JackpotGaming
{
    public class DebitOutput
    {
        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "debit")]
        public decimal Debit { get; set; }

        [JsonProperty(PropertyName = "credit")]
        public decimal Credit { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}