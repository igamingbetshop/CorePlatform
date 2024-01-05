using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.SunCity
{
   public class PlayerOutput
    {
        [JsonProperty(PropertyName = "authtoken")]
        public string AuthToken { get; set; }
    }
}
