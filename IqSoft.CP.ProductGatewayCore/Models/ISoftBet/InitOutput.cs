using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.ISoftBet
{
    public class InitOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "playerid")]
        public string PlayerId { get; set; }

        [JsonProperty(PropertyName = "sessionid")]
        public string SessionId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string CurrencId { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public long Balance { get; set; }

        ///// <summary>
        ///// false - test account
        ///// true - real account
        ///// </summary>
        //[JsonProperty(PropertyName = "realplayer")]
        //public string IsRealPlayer { get; set; }
    }
}