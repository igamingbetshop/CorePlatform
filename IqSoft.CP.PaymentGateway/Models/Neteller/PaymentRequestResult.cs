using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Neteller
{
    public class PaymentRequestResult
    {
        [JsonProperty("id")]
        public string OrderId { get; set; }

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

    public class Item
    {
        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }

    public class Redirect
    {
        [JsonProperty("rel")]
        public string ResultEvent { get; set; }

        [JsonProperty("uri")]
        public string ResultUri { get; set; }
    }

    public class Link
    {
        [JsonProperty("url")]
        public string RedirectUrl { get; set; }

        [JsonProperty("rel")]
        public string ResultEvent { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }
    }
}