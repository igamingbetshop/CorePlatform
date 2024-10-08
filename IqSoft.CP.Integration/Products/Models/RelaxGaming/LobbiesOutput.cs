using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.RelaxGaming
{
    public class LobbiesOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "lobbies")]
        public List<LobbyItem> Lobbies { get; set; }
    }

    public class LobbyItem
    {
        [JsonProperty(PropertyName = "lobbyid")]
        public string LobbyId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}