using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Neteller
{
    public class PaymentInput
    {
        [JsonProperty("order")]
        public Order OrderData { get; set; }             
    }

    public class Order
    {
        [JsonProperty("merchantRefId")]
        public string MerchantRefId { get; set; }

        [JsonProperty("totalAmount")]
        public int TotalAmount { get; set; }

        [JsonProperty("currency")]
        public string CurrencyId { get; set; }

        [JsonProperty("lang")]
        public string Language { get; set; }

        [JsonProperty("items")]
        public Item[] Items { get; set; }

        [JsonProperty("redirects")]
        public Redirect[] Redirects { get; set; }
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
        public int Amount { get; set; }
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
        [JsonProperty("href")]
        public string RedirectUrl { get; set; }

        [JsonProperty("rel")]
        public string ResultEvent { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }
    }
}
