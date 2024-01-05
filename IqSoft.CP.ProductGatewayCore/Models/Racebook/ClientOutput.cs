using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Racebook
{
    public class ClientOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "player")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "playerNickname")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "language")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "partner")]
        public string Partner { get; set; }

        [JsonProperty(PropertyName = "partnersTree")]
        public string PartnersTree { get; set; }
    }
}