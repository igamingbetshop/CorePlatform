using Newtonsoft.Json;
using System;

namespace IqSoft.CP.PaymentGateway.Models.Ngine
{
    public class PayoutRequest
    {

        [JsonProperty(PropertyName = "CustPIN")]
        public string CustPIN { get; set; }

        [JsonProperty(PropertyName = "CustPassword")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "Amount")]
        public decimal Amount { get; set; }
        [JsonProperty(PropertyName = "ProcessorName")]
        public string ProcessorName { get; set; }

        [JsonProperty(PropertyName = "TransactionID")]
        public int TransactionID { get; set; }

        [JsonProperty(PropertyName = "TransDate")]
        public DateTime TransDate { get; set; }

        [JsonProperty(PropertyName = "TransNote")]
        public string TransNote { get; set; }

        [JsonProperty(PropertyName = "IPAddress")]
        public string IPAddress { get; set; }

        [JsonProperty(PropertyName = "CurrencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty(PropertyName = "Fee")]
        public decimal Fee { get; set; }
    }

    public class PayoutInput
    {
        [JsonProperty(PropertyName = "PayoutRequest")]
        public PayoutRequest PayoutRequest { get; set; }
    }
}