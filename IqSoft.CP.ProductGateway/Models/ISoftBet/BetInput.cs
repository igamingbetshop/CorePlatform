using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.ISoftBet
{
    public class BetInput
    {
        [JsonProperty(PropertyName = "transactionid")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "roundid")]
        public string RoundId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public int Amount { get; set; }

        /// <summary>
        /// with 10 decimals, in cents
        /// </summary>
        [JsonProperty(PropertyName = "jpc")]
        public decimal JackpotContribution { get; set; }
    }
}