using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.Insic
{
    public class PlayerData : RequestBase
    {
        [JsonProperty(PropertyName = "personalPlayerData")]
        public PersonalPlayerData PersonalPlayerData { get; set; }
    }
}
