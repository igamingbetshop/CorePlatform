using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Evolution
{
    public class Game
    {
        [JsonProperty(PropertyName = "id")]
        public string RoundId { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "details")]
        public Details Details { get; set; }
    }

    public class Details
    {
        [JsonProperty(PropertyName = "table")]
        public Table Table { get; set; }
    }

    public class Table
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "vid")]
        public string Vid { get; set; }
    }
}