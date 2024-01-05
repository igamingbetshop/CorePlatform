using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Models.Neteller
{
    public class PayoutOutput
    {
        [JsonProperty("transaction")]
        public Transaction TransactionData { get; set; }

        [JsonProperty("paymentHandles")]
        public List<PaymentHandles> PaymentHandles { get; set; }
    }

    public class PaymentHandles
    {
        [JsonProperty("id")]
        public string ExternalTransactionId { get; set; }

        [JsonProperty("merchantRefNum")]
        public string MerchantRefNum { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("statusReason")]
        public string StatusReason { get; set; }
    }
}
