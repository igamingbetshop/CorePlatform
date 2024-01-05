using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.SkyWind
{
    public class ValidateTicketOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "cust_session_id")]
        public string CustomerSessionId { get; set; }

        [JsonProperty(PropertyName = "cust_id")]
        public string CustomerId { get; set; }

        [JsonProperty(PropertyName = "currency_code")]
        public string CurrencyCode { get; set; }

        [JsonProperty(PropertyName = "test_cust")]
        public string IsTest { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "language")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "game_group")]
        public string GameGoup { get; set; }
    }
}