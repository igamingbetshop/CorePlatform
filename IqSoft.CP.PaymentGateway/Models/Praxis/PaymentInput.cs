using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Praxis
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "merchant_id")]
        public string MerchantId { get; set; }

        [JsonProperty(PropertyName = "application_key")]
        public string ApplicationKey { get; set; }

        [JsonProperty(PropertyName = "conversion_rate")]
        public string ConversionRate { get; set; }

        [JsonProperty(PropertyName = "customer")]
        public Customer Customer { get; set; }

        [JsonProperty(PropertyName = "session")]
        public Session Session { get; set; }

        [JsonProperty(PropertyName = "transaction")]
        public Transaction Transaction { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public int Timestamp { get; set; }
    }
    public class Transaction
    {
        [JsonProperty(PropertyName = "transaction_type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "transaction_status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "tid")]
        public int Tid { get; set; }

        [JsonProperty(PropertyName = "transaction_id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "conversion_rate")]
        public string ConversionRate { get; set; }

        [JsonProperty(PropertyName = "processed_amount")]
        public string ProcessedAmount { get; set; }

        [JsonProperty(PropertyName = "processed_currency")]
        public string ProcessedCurrency { get; set; }

        [JsonProperty(PropertyName = "card")]
        public Card CardDetails { get; set; }

        [JsonProperty(PropertyName = "wallet")]
        public Wallet WalletDetails { get; set; }
    }

    public class Session
    {
        [JsonProperty(PropertyName = "auth_token")]
        public string AuthToken { get; set; }

        [JsonProperty(PropertyName = "intent")]
        public string Intent { get; set; }

        [JsonProperty(PropertyName = "session_status")]
        public string SessionStatus { get; set; }

        [JsonProperty(PropertyName = "order_id")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "payment_method")]
        public string PaymentMethod { get; set; }

        [JsonProperty(PropertyName = "gateway")]
        public string Gateway { get; set; }
    }

    public class Customer
    {
        [JsonProperty(PropertyName = "customer_token")]
        public string Token { get; set; }
    }

    public class Card
    {
        [JsonProperty(PropertyName = "card_token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "card_type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "card_number")]
        public string CardNumber { get; set; }

        [JsonProperty(PropertyName = "card_exp")]
        public string ExpDate { get; set; }

        [JsonProperty(PropertyName = "card_issuer_name")]
        public string BankName { get; set; }
    }

    public class Wallet
    {
        [JsonProperty(PropertyName = "card_token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "account_identifier")]
        public string AccountIdentifier { get; set; }
    }
}
