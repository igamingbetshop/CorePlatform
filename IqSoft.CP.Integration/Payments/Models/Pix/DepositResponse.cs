using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Pix
{
    public class DepositResponse
    {
        [JsonProperty(PropertyName = "encodedValue")]
        public string EncodedValue { get; set; }
    }
}
