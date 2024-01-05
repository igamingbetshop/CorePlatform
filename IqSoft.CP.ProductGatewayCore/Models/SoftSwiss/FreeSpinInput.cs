using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.SoftSwiss
{
    public class FreeSpinInput
    {
        [JsonProperty(PropertyName = "issue_id")]
        public string BonusData { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "total_amount")]
        public string TotalAmount { get; set; }
    }
}