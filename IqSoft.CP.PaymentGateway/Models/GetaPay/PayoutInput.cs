using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.PaymentGateway.Models.GetaPay
{
    public class PayoutInput
    {
        [JsonProperty(PropertyName = "order_id")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "payout_id")]
        public string  PayoutId { get; set; }

        [JsonProperty(PropertyName = "payout_status")]
        public string PayoutStatus { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "full_amount")]
        public string FullAmount { get; set; }

        [JsonProperty(PropertyName = "amount_gate")]
        public string AmountGate { get; set; }

        [JsonProperty(PropertyName = "status_code")]
        public string StatusCode { get; set; }

        [JsonProperty(PropertyName = "status_description")]
        public string StatusDescription { get; set; }

        [JsonProperty(PropertyName = "currency_gate")]
        public string CurrencyGate { get; set; }

        [JsonProperty(PropertyName = "success")]
        public string Success { get; set; }

        [JsonProperty(PropertyName = "signature")]
        public string Signature { get; set; }
    }
}