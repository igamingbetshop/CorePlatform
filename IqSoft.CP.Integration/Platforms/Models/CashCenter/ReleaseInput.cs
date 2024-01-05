using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.CashCenter
{
    public class ReleaseInput
    {
        [JsonProperty(PropertyName = "confirmed")]
        public bool Confirmed { get; set; }

        [JsonProperty(PropertyName = "refId")]
        public string TransactionId { get; set; }
    }
}
