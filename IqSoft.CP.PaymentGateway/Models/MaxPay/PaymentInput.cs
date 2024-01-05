using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.PaymentGateway.Models.MaxPay
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "amount_payed")]
        public string AmountPayed { get; set; }

        [JsonProperty(PropertyName = "amount_merchant")]
        public string AmountMerchant { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "payment_system")]
        public string PaymentSystem { get; set; }

        [JsonProperty(PropertyName = "created")]
        public int Created { get; set; }

        [JsonProperty(PropertyName = "updated")]
        public int Updated { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "original_currency")]
        public string OriginalCurrency { get; set; }

        [JsonProperty(PropertyName = "original_amount")]
        public string OriginalAmount { get; set; }

        [JsonProperty(PropertyName = "original_transaction")]
        public OriginalTransaction OriginalTransaction { get; set; }

        [JsonProperty(PropertyName = "fee")]
        public string Fee { get; set; }

        [JsonProperty(PropertyName = "rolling_reserve")]
        public string RollingReserve { get; set; }

        [JsonProperty(PropertyName = "compensation")]
        public string Compensation { get; set; }

        [JsonProperty(PropertyName = "recurring_id")]
        public int RecurringId { get; set; }

        [JsonProperty(PropertyName = "system_fields")]
        public object SystemFields { get; set; }
    }

    public class OriginalTransaction
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "transaction_id")]
        public string TransactionId { get; set; }
    }   

}