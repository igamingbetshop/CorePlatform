using IqSoft.CP.Common.Helpers;
using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.TomHorn
{
    public class BetInput 
    {
        [JsonProperty(PropertyName = "partnerID")]
        public string OperatorId { get; set; }

        [JsonProperty(PropertyName = "sign")]
        public string Sign { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        [JsonConverter(typeof(DecimalJsonConverter))]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "reference")]
        public long? TransactionId { get; set; }

        [JsonProperty(PropertyName = "sessionID")]
        public long? SessionId { get; set; }

        [JsonProperty(PropertyName = "gameRoundID")]
        public long? GameRoundId { get; set; }

        [JsonProperty(PropertyName = "gameModule")]
        public string GameModule { get; set; }

        [JsonProperty(PropertyName = "type")]
        public int? Type { get; set; }

        [JsonProperty(PropertyName = "fgbCampaignCode")]
        public string fgbCampaignCode { get; set; }

    }
}