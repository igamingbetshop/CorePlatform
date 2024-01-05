using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Instapay
{
    public class CepbankInput
    {
        [JsonProperty(PropertyName = "transaction_type")]
        public string TransactionType { get; set; }

        [JsonProperty(PropertyName = "eps_tran_ref")]
        public string PaymentRequestId { get; set; }

        [JsonProperty(PropertyName = "customer_ref")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "inst_tran_ref")]
        public string InstTransactionId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "date_time")]
        public string DateTime { get; set; }

        [JsonProperty(PropertyName = "atm_fee")]
        public decimal? AtmFee { get; set; }

        [JsonProperty(PropertyName = "note")]
        public string Note { get; set; }

        [JsonProperty(PropertyName = "hash")]
        public string Hash { get; set; }
    }
}