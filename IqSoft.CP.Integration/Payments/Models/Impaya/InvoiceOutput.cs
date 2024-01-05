using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Impaya
{
    public class InvoiceOutput
    {
        [JsonProperty(PropertyName = "response")]
        public string Response { get; set; }

        [JsonProperty(PropertyName = "status_id")]
        public int StatusId { get; set; }

        [JsonProperty(PropertyName = "status_descr")]
        public string StatusDescrition { get; set; }

        [JsonProperty(PropertyName = "3ds")]
        public _3Ds _3DS { get; set; }

        [JsonProperty(PropertyName = "transaction")]
        public Transaction TransactionDetails { get; set; }
    }

    public class _3Ds
    {
        [JsonProperty(PropertyName = "url")]
        public string RedirectUrl { get; set; }

        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }
    }

    public class Transaction
    {
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }


        [JsonProperty(PropertyName = "merchant_id")]
        public string MerchantId { get; set; }


        [JsonProperty(PropertyName = "mc_transaction_id")]
        public string MerchantTransactionId { get; set; }


        [JsonProperty(PropertyName = "transaction_id")]
        public string TransactionId { get; set; }


        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }


        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }


        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }


        [JsonProperty(PropertyName = "payment_system")]
        public string PaymentSystem { get; set; }


        [JsonProperty(PropertyName = "status_id")]
        public int StatusId { get; set; }


        [JsonProperty(PropertyName = "payment_system_status")]
        public string PaymentSystemStatus { get; set; }


        [JsonProperty(PropertyName = "hash")]
        public string Hash { get; set; }
    }
}
