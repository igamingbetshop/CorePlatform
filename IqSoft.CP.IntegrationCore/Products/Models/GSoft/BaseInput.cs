using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.GSoft
{
    public class BaseInput
    {
        [JsonProperty(PropertyName = "vendor_id")]
        public string vendor_id { get; set; }

        [JsonProperty(PropertyName = "vendor_member_id")]
        public string vendor_member_id { get; set; }

        [JsonProperty(PropertyName = "vendor_member_ids")]
        public string vendor_member_ids { get; set; }
    }
}