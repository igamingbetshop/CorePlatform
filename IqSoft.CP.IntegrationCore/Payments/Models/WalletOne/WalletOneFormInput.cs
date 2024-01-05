using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.WalletOne
{
    public class WalletOneFormInput : WalletOneFinalInput
    {
        [JsonProperty(PropertyName = "Params")]
        public WalletOneFormBaseFields[] Params { get; set; }
    }
}
