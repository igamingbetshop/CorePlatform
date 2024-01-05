using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.PremierCashier
{
    public class PaymentInput : BaseInput
    {

        [JsonProperty(PropertyName = "lang")]
        public string Lang { get; set; }

        [JsonProperty(PropertyName = "crm")]
        public Crm Crm { get; set; }

        [JsonProperty(PropertyName = "transaction")]
        public Transaction Transaction { get; set; }

        [JsonProperty(PropertyName = "processor")]
        public ProcessorModel Processor { get; set; }
    }

    public class ProcessorModel
    {
        [JsonProperty(PropertyName = "traceid")]
        public string Traceid { get; set; }

        [JsonProperty(PropertyName = "pp_amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "pp_currency_code")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "status_code")]
        public string StatusCode { get; set; }

        [JsonProperty(PropertyName = "status_message")]
        public string StatusMessage { get; set; }
    }

    public class Crm
    {
        [JsonProperty(PropertyName = "order_id")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }
    }

    public class Transaction
    {
        [JsonProperty(PropertyName = "traceid")]
        public string TraceId { get; set; }

        [JsonProperty(PropertyName = "tran_type")]
        public string TranType { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "card")]
        public Card CardDetails { get; set; }

        [JsonProperty(PropertyName = "wallet")]
        public Wallet WalletDetails { get; set; }
    }

    public class Card
    {
        [JsonProperty(PropertyName = "payment_method")]
        public string PaymentMethod { get; set; }

        [JsonProperty(PropertyName = "cardholder_name")]
        public string CardHolderName { get; set; }

        [JsonProperty(PropertyName = "card_number_masked")]
        public string CardNumberMasked { get; set; }

        [JsonProperty(PropertyName = "card_exp")]
        public string CardExp { get; set; }
    }
    public class Wallet
    {
        [JsonProperty(PropertyName = "payment_method")]
        public string PaymentMethod { get; set; }

        [JsonProperty(PropertyName = "account_identifier")]
        public string AccountIdentifier { get; set; }
    }

    }