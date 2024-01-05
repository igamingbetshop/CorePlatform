using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Integration.Payments.Models.MaxPay
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string ExternalId { get; set; }

        [JsonProperty(PropertyName = "transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string TransactionType { get; set; }

        [JsonProperty(PropertyName = "amount_to_pay")]
        public string AmountToPay { get; set; }

        [JsonProperty(PropertyName = "amount_merchant")]
        public string AmountMerchant { get; set; }

        [JsonProperty(PropertyName = "amount_client")]
        public string AmountClient { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "payment_system")]
        public string PaymentSystem { get; set; }

        [JsonProperty(PropertyName = "redirect")]
        public Redirect Redirect { get; set; }

        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "system_fields")]
        public object  SystemFields { get; set; }

        [JsonProperty(PropertyName = "created")]
        public int Created { get; set; }
    }
    public class Redirect
    {
        [JsonProperty(PropertyName = "url")]
        public string  Url { get; set; }

        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "params")]
        public object Params { get; set; }
    }
}
