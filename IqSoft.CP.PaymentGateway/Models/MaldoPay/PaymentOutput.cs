using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.PaymentGateway.Models.MaldoPay
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "transaction")]
        public Transaction Transaction { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "conversion")]
        public Conversion Conversion { get; set; }

        [JsonProperty(PropertyName = "checksum")]
        public string Checksum { get; set; }

        [JsonProperty(PropertyName = "next_step")]
        public string NextStep { get; set; }

        [JsonProperty(PropertyName = "redirect")]
        public string Redirect { get; set; }

        [JsonProperty(PropertyName = "send_sms_link ")]
        public string SendSmsLink { get; set; }
    }

    public class Transaction
    {
        [JsonProperty(PropertyName = "codeId")]
        public int CodeId { get; set; }

        [JsonProperty(PropertyName = "codeMessage")]
        public string CodeMessage { get; set; }

        [JsonProperty(PropertyName = "serviceId")]
        public string ServiceId { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }
    }    

}