using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.WalletOne
{
    public class WalletOneFinalInput
    {
        [JsonProperty(PropertyName = "FormId")]
        public string FormId { get; set; }
    }
}
