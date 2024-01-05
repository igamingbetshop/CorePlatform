using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Payments.Models.Pay3000
{
	internal class PaymentOutput
    {
        [JsonProperty(PropertyName = "paymentRequestId")]
        public string PaymentRequestId { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public double Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "operationType")]
        public string OperationType { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "customerName")]
        public string CustomerName { get; set; }

        [JsonProperty(PropertyName = "merchantTitle")]
        public string MerchantTitle { get; set; }
    }
}
