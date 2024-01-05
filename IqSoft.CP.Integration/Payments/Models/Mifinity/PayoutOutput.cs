using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Models.Mifinity
{
    public class PayoutOutput
    {
        [JsonProperty(PropertyName = "payload")]
        public List<PayoutPayload> Payload { get; set; }
    }

    public class PayoutPayload
    {
        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "transactionReference")]
        public string TransactionReference { get; set; }

        [JsonProperty(PropertyName = "traceId")]
        public string TraceId { get; set; }

        [JsonProperty(PropertyName = "exchangeRate")]
        public string ExchangeRate { get; set; }

        [JsonProperty(PropertyName = "totalFees")]
        public FeeDetails TotalFees { get; set; }
    }

    public class FeeDetails
    {
        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }
    }

}
