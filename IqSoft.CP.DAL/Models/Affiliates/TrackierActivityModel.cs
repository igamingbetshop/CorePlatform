using Newtonsoft.Json;

namespace IqSoft.CP.DAL.Models.Affiliates
{
    public class TrackierActivityModel
    {

        [JsonProperty(PropertyName = "customerId")]
        public string CustomerId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty(PropertyName = "date")]
        public string Date { get; set; }

        [JsonProperty(PropertyName = "productId")]
        public string ProductId { get; set; }

        [JsonProperty(PropertyName = "deposits")]
        public decimal Deposits { get; set; }

        [JsonProperty(PropertyName = "withdrawls")]
        public decimal Withdrawls { get; set; }

        [JsonProperty(PropertyName = "bets")]
        public decimal Bets { get; set; }

        [JsonProperty(PropertyName = "wins")]
        public decimal Wins { get; set; }

        [JsonProperty(PropertyName = "adjustments")]
        public decimal Adjustments { get; set; }

        [JsonProperty(PropertyName = "fees")]
        public decimal Fees { get; set; }

        [JsonProperty(PropertyName = "bonuses")]
        public decimal Bonuses { get; set; }   

        [JsonProperty(PropertyName = "chargebacks")]
        public decimal Chargebacks { get; set; }
    }
}