using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Piastrix
{
    public class CheckStatusInput
    {
        [JsonProperty(PropertyName = "now")]
        public string CurrentDataTime { get; set; }

        [JsonProperty(PropertyName = "shop_id")]
        public string RequestId { get; set; }

        [JsonProperty(PropertyName = "withdraw_id")]
        public string WithdrawId { get; set; }

        [JsonProperty(PropertyName = "sign")]
        public string Sign { get; set; }
    }
}
