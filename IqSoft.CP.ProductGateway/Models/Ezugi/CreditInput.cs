using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Ezugi
{
    public class CreditInput : GeneralInput
    {
        [JsonProperty(PropertyName = "betTypeID")]
        public string BetTypeID { get; set; }

        [JsonProperty(PropertyName="creditAmount")]
        public decimal CreditAmount { get; set; }

        [JsonProperty(PropertyName="returnReason")]
        public int ReturnReason { get; set; }

        [JsonProperty(PropertyName="isEndRound")]
        public bool IsEndRound { get; set; }

        [JsonProperty(PropertyName = "gameDataString")]
        public string GameDataString { get; set; }

        [JsonProperty(PropertyName = "creditIndex")]
        public string CreditIndex { get; set; }

        [JsonProperty(PropertyName = "debitTransactionId")]
        public string DebitTransactionId { get; set; }
    }
}