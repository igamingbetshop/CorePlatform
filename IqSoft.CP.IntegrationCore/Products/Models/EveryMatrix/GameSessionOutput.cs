using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.EveryMatrix
{
    public class GameSessionOutput
    {
        public string EmUserId { get; set; }
        public string OperatorUserId { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        [JsonProperty(PropertyName = "ceSession")]
        public string SessionId { get; set; }
    }
}
