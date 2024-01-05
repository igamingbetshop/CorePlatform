using Newtonsoft.Json;

namespace IqSoft.CP.IntegrationCore.Payments.Models.Pix
{
    public class DepositResponse
    {
        [JsonProperty(PropertyName = "encodedValue")]
        public string EncodedValue { get; set; }
    }
}
