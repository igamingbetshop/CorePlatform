using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.FugaPay
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "transaction")]
        public Transaction TransactionDetails { get; set; }

        [JsonProperty(PropertyName = "Desc")]
        public string Desc { get; set; }

        [JsonProperty(PropertyName = "signature")]
        public string Signature { get; set; }
    }

    public class Transaction
    {
        [JsonProperty(PropertyName = "RequestId")]
        public string TrnRequestId { get; set; }

        [JsonProperty(PropertyName = "OrderID")]
        public string OrderID { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "Amount")]
        public decimal Amount { get; set; }  
    }
}