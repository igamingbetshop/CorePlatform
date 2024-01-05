using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.Rocabee
{
    internal class LobbyUrlRequest
    {
        [JsonProperty(PropertyName = "userId")]
        public int ClientId { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName= "forename")]
        public string Forename { get; set; }

        [JsonProperty(PropertyName = "surname")]
        public string Surename { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
    }
}