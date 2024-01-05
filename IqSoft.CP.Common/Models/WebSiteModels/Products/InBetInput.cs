using Newtonsoft.Json;

namespace IqSoft.CP.Common.Models.WebSiteModels.Products
{
    public class InBetInput
    {
        [JsonProperty(PropertyName = "customer_id")]
        public string CustomerId { get; set; }

        [JsonProperty(PropertyName = "session")]
        public string Session { get; set; }

        [JsonProperty(PropertyName = "denomination")]
        public int Denomination { get; set; }

        [JsonProperty(PropertyName = "application")]
        public string ProductExternalId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "language")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "home_page")]
        public string WebSiteUrl { get; set; }

        [JsonProperty(PropertyName = "path_to_storage")]
        public string ProxyFilePath { get; set; }
    }
}