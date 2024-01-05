using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Interac
{
    public class PayoutResult
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "data")]
        public DataModel Data { get; set; }

        [JsonProperty(PropertyName = "err")]
        public string Error { get; set; }
    }
}