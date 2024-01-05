using Newtonsoft.Json;
using System;

namespace IqSoft.CP.PaymentGateway.Models.FinalPay
{
    public class PaymentRequestInput
    {
        [JsonProperty(PropertyName = "checksum")]
        public string CheckSum { get; set; }

        [JsonProperty(PropertyName = "data")]
        public Data DataDetails { get; set; }

        public class Data
        {
            [JsonProperty(PropertyName = "pay_type")]
            public string PayType { get; set; }

            [JsonProperty(PropertyName = "request_type")]
            public string RequestType { get; set; }

            [JsonProperty(PropertyName = "trans_ref")]
            public string OrderId { get; set; }

            [JsonProperty(PropertyName = "status")]
            public string Status { get; set; }

            [JsonProperty(PropertyName = "status_timestamp")]
            public string Timestamp { get; set; }

            [JsonProperty(PropertyName = "code")]
            public int Code { get; set; }

            [JsonProperty(PropertyName = "code_msg")]
            public string Message { get; set; }

            [JsonProperty(PropertyName = "amount")]
            public string Amount { get; set; }

            [JsonProperty(PropertyName = "currency")]
            public string Currency { get; set; }
        }
    }
}