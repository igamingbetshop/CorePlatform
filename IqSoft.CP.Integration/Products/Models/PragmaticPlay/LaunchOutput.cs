using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.PragmaticPlay
{
    public class LaunchOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "gameURL")]
        public string GameURL { get; set; }
    }
}