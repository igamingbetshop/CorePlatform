using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Racebook
{
    public class WinInput : BetInput
    {
        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "isParcial")]
        public bool IsParcial { get; set; }
    }
}