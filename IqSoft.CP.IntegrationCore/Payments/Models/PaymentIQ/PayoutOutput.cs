using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Models.PaymentIQ
{
    public class PayoutOutput
    {
        [JsonProperty(PropertyName = "txState")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "txRefId")]
        public string TxRefId { get; set; }

        [JsonProperty(PropertyName = "txId")]
        public string TxId { get; set; }

        [JsonProperty(PropertyName = "statusCode")]
        public string StatusCode { get; set; }

        [JsonProperty(PropertyName = "maskedAccount")]
        public string MaskedAccount { get; set; }

        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "errors")]
        public List<object> Errors { get; set; }
    }
}
