using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.PaymentGateway.Models.Mifinity
{
    public class StatusOutput
    {
        [JsonProperty(PropertyName = "payload")]
        public List<Payload> PayloadData { get; set; }
    }

    public class Payload
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "paymentResponse")]
        public PaymentResponse PaymentDetails { get; set; }

        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; }
    }

    public class PaymentResponse
    {
        [JsonProperty(PropertyName = "transactionReference")]
        public string TransactionReference { get; set; }
    }
}