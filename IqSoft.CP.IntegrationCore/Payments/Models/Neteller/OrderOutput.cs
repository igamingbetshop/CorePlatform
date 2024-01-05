using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Neteller
{
    public class OrderOutput
    {
        [JsonProperty("id")]
        public string ExternalTransactionId { get; set; }

        [JsonProperty("merchantRefId")]
        public string MerchantRefId { get; set; }

        [JsonProperty("totalAmount")]
        public int TotalAmount { get; set; }

        [JsonProperty("currency")]
        public string CurrencyId { get; set; }

        [JsonProperty("lang")]
        public string Language { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("items")]
        public Item[] Items { get; set; }

        [JsonProperty("redirects")]
        public Redirect[] Redirects { get; set; }

        [JsonProperty("links")]
        public Link[] Links { get; set; }
    }
}
