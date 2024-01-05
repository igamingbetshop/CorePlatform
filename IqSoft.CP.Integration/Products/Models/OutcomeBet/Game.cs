using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.OutcomeBet
{
    public class Game
    {
        [JsonProperty(PropertyName = "jsonrpc")]
        public string JsonRpc { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string RequestId { get; set; }

        [JsonProperty(PropertyName = "result")]
        public ResultModel Result { get; set; }
  }
    public class ResultModel
    {
        public List<GameItem> Games { get; set; }
    }

    public class GameItem
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string SectionId { get; set; }
        public string Type { get; set; }
    }
}
