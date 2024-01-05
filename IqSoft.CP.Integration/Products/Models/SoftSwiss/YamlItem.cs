using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Products.Models.SoftSwiss
{
    public class YamlItem
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("identifier2")]
        public string Identifier2 { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("producer")]
        public string Producer { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("has_freespins")]
        public bool HasFreespins { get; set; }

        [JsonProperty("feature_group")]
        public string FeatureGroup { get; set; }

        [JsonProperty("devices")]
        public Device[] Devices { get; set; }

        [JsonProperty("payout", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Payout { get; set; }

        public bool IsMobile { get; set; }
        public bool IsDesktop { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("volatility_rating")]
        public string VolatilityRating { get; set; }

        [JsonProperty("hd")]
        public bool Hd { get; set; }

        [JsonProperty("released_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? ReleasedAt { get; set; }

        [JsonProperty("restrictions")]
        public Restrictions Restrictions { get; set; }

        [JsonProperty("lines", NullValueHandling = NullValueHandling.Ignore)]
        public long? Lines { get; set; }

        [JsonProperty("accumulating", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Accumulating { get; set; }

        [JsonProperty("multiplier", NullValueHandling = NullValueHandling.Ignore)]
        public long? Multiplier { get; set; }

        [JsonProperty("ways", NullValueHandling = NullValueHandling.Ignore)]
        public long? Ways { get; set; }
    }

    public partial class Restrictions
    {
        [JsonProperty("default")]
        public Default Default { get; set; }
    }

    public partial class Default
    {
        [JsonProperty("blacklist")]
        public object[] Blacklist { get; set; }
    }

    public enum Category { Card, Casual, Craps, Lottery, Poker, Roulette, Slots, VideoPoker };

    public enum Device { Desktop, Mobile };

    public enum FeatureGroup { Basic, New };

    public enum Producer { Bgaming };

    public enum Provider { Softswiss };
}
