using Newtonsoft.Json;
using System;

namespace IqSoft.CP.ProductGateway.Models.EkkoSpin
{
    public class BasicInput
    {
        [JsonProperty(PropertyName = "id_customer")]
        public int ClientId { get; set; }

        [JsonProperty(PropertyName = "id_game")]
        public int GameId { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTime RequestDate { get; set; }
    }
}