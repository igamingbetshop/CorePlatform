using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.PaymentGateway.Models.Instapay
{
    public class ExpressHavaleInput
    {
        [JsonProperty(PropertyName = "transaction_type")]
        public string TransactionType { get; set; }

        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

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

        [JsonProperty(PropertyName = "error_message")]
        public string ErrorMessage { get; set; }

        [JsonProperty(PropertyName = "hash")]
        public string Hash { get; set; }
    }
}