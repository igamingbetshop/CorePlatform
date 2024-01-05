using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.WalletOne
{
    public class RequestInput
    {
        [JsonProperty(PropertyName = "Content")]
        public string Content { get; set; }
    }
}
