using IqSoft.CP.ProductGateway.Models.ISoftBet;
using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.SolidGaming
{
    public class AuthenticationOutput
    {
        [JsonProperty(PropertyName = "responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty(PropertyName = "playerId")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "playerUsername")]
        public string PlayerUsername { get; set; }

        [JsonProperty(PropertyName = "brandCode")]
        public string BrandCode { get; set; }

        [JsonProperty(PropertyName = "currencyCode")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "languageCode")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "countryCode")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "balance")]
        [JsonConverter(typeof(DecimalJsonConverter))]
        public decimal Balance { get; set; }
    }
}