using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.WalletOne
{
    public class RequestInput
    {
        [JsonProperty(PropertyName = "Content")]
        public string Content { get; set; }
    }
}