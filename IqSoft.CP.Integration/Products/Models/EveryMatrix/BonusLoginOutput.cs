using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.EveryMatrix
{
    public class BonusLoginOutput
    {
        [JsonProperty(PropertyName = "sessionID")]
        public string SessionId { get; set; }
    }
}
