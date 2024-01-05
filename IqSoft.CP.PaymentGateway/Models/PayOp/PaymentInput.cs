using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.PayOp
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "invoice")]
        public Invoice InvoiceDetails { get; set; }

        [JsonProperty(PropertyName = "transaction")]
        public Transaction TransactionDetails { get; set; }
    }

    public class Invoice
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "txid")]
        public string TransactionId { get; set; }
    }

    public class Transaction
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "state")]
        public int State { get; set; }

        [JsonProperty(PropertyName = "order")]
        public Order OrderDetails { get; set; }

        [JsonProperty(PropertyName = "error")]
        public Error ErrorDetails { get; set; }
    }

    public class Order
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }

    public class Error
    {
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }
    }
}