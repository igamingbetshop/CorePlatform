using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.PaymentGateway.Models.CoinsPaid
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "foreign_id")]
        public string ForeignId { get; set; }

        [JsonProperty(PropertyName = "crypto_address")]
        public CryptoAddress CryptoAddress { get; set; }

        [JsonProperty(PropertyName = "currency_sent")]
        public CurrencySent CurrencySent { get; set; }

        [JsonProperty(PropertyName = "currency_received")]
        public CurrencyReceived CurrencyReceived { get; set; }

        [JsonProperty(PropertyName = "transactions")]
        public List<Transaction> Transactions { get; set; }

        [JsonProperty(PropertyName = "fees")]
        public List<Fee> Fees { get; set; }

        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
    public class CryptoAddress
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "convert_to")]
        public string ConvertTo { get; set; }

        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "tag")]
        public object Tag { get; set; }

        [JsonProperty(PropertyName = "foreign_id")]
        public string ForeignId { get; set; }
    }

    public class CurrencyReceived
    {
        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "amount_minus_fee")]
        public string AmountMinusFee { get; set; }
    }

    public class CurrencySent
    {
        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }
    }

    public class Fee
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }
    }

    public class Transaction
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "transaction_type")]
        public string TransactionType { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "tag")]
        public object Tag { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "txid")]
        public string Txid { get; set; }

        [JsonProperty(PropertyName = "riskscore")]
        public string Riskscore { get; set; }

        [JsonProperty(PropertyName = "confirmations")]
        public string Confirmations { get; set; }

        [JsonProperty(PropertyName = "currency_to")]
        public string CurrencyTo { get; set; }

        [JsonProperty(PropertyName = "amount_to")]
        public string AmountTo { get; set; }
    }
}