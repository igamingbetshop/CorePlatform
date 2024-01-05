using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Products.Models.GSoft
{
    public class BalanceHistoryData
    {
        [JsonProperty(PropertyName = "Transid")]
        public int TransactionExternalId { get; set; }

        [JsonProperty(PropertyName = "TransDate")]
        public DateTime TransactionDate { get; set; }

        [JsonProperty(PropertyName = "Type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "BetType")]
        public string BetType { get; set; }

        [JsonProperty(PropertyName = "Stake")]
        public string Stake { get; set; }

        [JsonProperty(PropertyName = "Winlost")]
        public string Winlost { get; set; }

        [JsonProperty(PropertyName = "Odds")]
        public string Odds { get; set; }

        [JsonProperty(PropertyName = "Status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "Currency")]
        public int Currency { get; set; }

        [JsonProperty(PropertyName = "SportType")]
        public int SportType { get; set; }
    }
}