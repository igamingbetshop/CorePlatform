using Newtonsoft.Json;
using System;

namespace IqSoft.CP.PaymentGateway.Models.Pay3000
{
	public class PaymentInput
    {
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "customerAliasId")]
        public string CustomerAliasId { get; set; }

        [JsonProperty(PropertyName = "operatorCommission")]
        public decimal OperatorCommission { get; set; }

        [JsonProperty(PropertyName = "paymentRequestId")]
        public string PaymentRequestId { get; set; }

        [JsonProperty(PropertyName = "requestDate")]
        public DateTime RequestDate { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }
}