using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Singular
{
    public class DepositInput : WithdrawInput
    {
        [JsonProperty(PropertyName = "isCardVerification")]
        public bool IsCardVerification { get; set; }

        [JsonProperty(PropertyName = "requestorIP")]
        public string RequestorIp { get; set; }
    }
}