using Newtonsoft.Json;
namespace IqSoft.CP.ProductGateway.Models.Habanero
{
    public class AltFundsInput
    {
        [JsonProperty(PropertyName = "accountid")]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "altcredittype")]
        public int AltCreditType { get; set; }

        [JsonProperty(PropertyName = "transferid")]
        public string TransferId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currencycode")]
        public string CurrencyCode { get; set; }
    }
}