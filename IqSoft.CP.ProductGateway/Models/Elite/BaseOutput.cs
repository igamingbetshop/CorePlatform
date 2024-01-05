using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Elite
{
    public class Result
    {
        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "cashBalance")]
        public decimal CashBalance { get; set; }

        [JsonProperty(PropertyName = "bonusBalance")]
        public decimal BonusBalance { get; set; }

        [JsonProperty(PropertyName = "conversionRate")]
        public string ConversionRate { get; set; } = "1";

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "cash")]
        public decimal Cash { get; set; }

        [JsonProperty(PropertyName = "cashCredit")]
        public decimal CashCredit { get; set; }

        [JsonProperty(PropertyName = "cashDebit")]
        public decimal CashDebit { get; set; }

        [JsonProperty(PropertyName = "bonus")]
        public decimal Bonus { get; set; }

        [JsonProperty(PropertyName = "bonusDebit")]
        public decimal BonusDebit { get; set; } = 0;

        [JsonProperty(PropertyName = "bonusCredit")]
        public decimal BonusCredit { get; set; } = 0;

        [JsonProperty(PropertyName = "externalTransactionId")]
        public string ExternalTransactionId { get; set; }

        [JsonProperty(PropertyName = "externalCreditTransactionId")]
        public string ExternalCreditTransactionId { get; set; }

        [JsonProperty(PropertyName = "externalDebitTransactionId")]
        public string ExternalDebitTransactionId { get; set; }

        [JsonProperty(PropertyName = "externalPromoWinTransactionId")]
        public string ExternalPromoWinTransactionId { get; set; }
    }

    public class BaseOutput
    {
        [JsonProperty(PropertyName = "result")]
        public Result Result { get; set; }
    }
}