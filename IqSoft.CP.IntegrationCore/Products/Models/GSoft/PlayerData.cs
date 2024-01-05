using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.GSoft
{
    public class PlayerData
    {
        [JsonProperty(PropertyName = "vendor_member_id")]
        public string PlayerName { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "outstanding")]
        public decimal Outstanding { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public int Currency { get; set; }
    }
}
