using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Habanero
{
    public class CommonInput : BaseInput
    {
        [JsonProperty(PropertyName = "basegame")]
        public Game GameDetails { get; set; }

        [JsonProperty(PropertyName = "gamedetails")]
        public GameSession GameSessionDetails { get; set; }

        [JsonProperty(PropertyName = "auth")]
        public Authentication AuthenticationDetails { get; set; }

        [JsonProperty(PropertyName = "playerdetailrequest")]
        public Player PlayerDetails { get; set; }

        [JsonProperty(PropertyName = "fundtransferrequest")]
        public TransferInput TransferDetails { get; set; }

        [JsonProperty(PropertyName = "queryrequest")]
        public QueryInput QueryDetails { get; set; }

        [JsonProperty(PropertyName = "altfundsrequest")]
        public AltFundsInput AltFundsDetails { get; set; }
    }
}