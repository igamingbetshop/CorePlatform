using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.WalletOne
{
    public class WalletOnePaymentInput
    {
        [JsonProperty(PropertyName = "ProviderId")]
        public string ProviderId { get; set; }

        [JsonProperty(PropertyName = "ExternalId")]
        public string ExternalId { get; set; }
    }
}
