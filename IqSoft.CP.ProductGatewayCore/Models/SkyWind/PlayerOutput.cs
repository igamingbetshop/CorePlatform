using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.SkyWind
{
    public class PlayerOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "cust_id")]
        public string CustomerId { get; set; }

        [JsonProperty(PropertyName = "game_group")]
        public string GameGroup { get; set; }

        [JsonProperty(PropertyName = "cust_login")]
        public string CustomerLogin { get; set; }

        [JsonProperty(PropertyName = "currency_code")]
        public string CurrencyCode { get; set; }

        [JsonProperty(PropertyName = "language")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = " test_cus")]
        public string IsTestCusomer { get; set; }
    }
}