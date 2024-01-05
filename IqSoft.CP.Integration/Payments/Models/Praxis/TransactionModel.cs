using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Praxis
{
    public class TransactionModel
    {
        [JsonProperty(PropertyName = "transaction_type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "transaction_status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "status_details")]
        public string StatusDescription { get; set; }

        [JsonProperty(PropertyName = "tid")]
        public int? Tid { get; set; }

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
