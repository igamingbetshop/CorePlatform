using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.EveryMatrix
{
    public class GameItem
    {
        [JsonProperty(PropertyName = "domainID")]
        public int DomainID { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "seq")]
        public int Seq { get; set; }

        [JsonProperty(PropertyName = "data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "slug")]
        public string Slug { get; set; }

        [JsonProperty(PropertyName = "vendor")]
        public string Vendor { get; set; }

        [JsonProperty(PropertyName = "vendorID")] 
        public int VendorID { get; set; }

        [JsonProperty(PropertyName = "vendorDisplayName")]
        public string VendorDisplayName { get; set; }

        [JsonProperty(PropertyName = "contentProvider")]
        public string ContentProvider { get; set; }

        [JsonProperty(PropertyName = "originalVendor")]
        public string OriginalVendor { get; set; }

        [JsonProperty(PropertyName = "gameCode")]
        public string GameCode { get; set; }

        [JsonProperty(PropertyName = "gameBundleID")]
        public string GameBundleID { get; set; }

        [JsonProperty(PropertyName = "gameID")]
        public string GameID { get; set; }

        [JsonProperty(PropertyName = "enabled")]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "categories")]
        public List<string> Categories { get; set; }

          [JsonProperty(PropertyName = "restrictedTerritories")]
        public List<string> RestrictedTerritories { get; set; }

        [JsonProperty(PropertyName = "theoreticalPayOut")]
        public decimal? TheoreticalPayOut { get; set; }

        [JsonProperty(PropertyName = "playMode")]
        public PlayModeModel PlayMode { get; set; }

        [JsonProperty(PropertyName = "presentation")]
        public PresentationModel Presentation { get; set; }

        [JsonProperty(PropertyName = "property")]
        public PropertyModel Property { get; set; }

        public class PlayModeModel
        {
            [JsonProperty(PropertyName = "fun")]
            public bool Fun { get; set; }

            [JsonProperty(PropertyName = "realMoney")]
            public bool RealMoney { get; set; }

            [JsonProperty(PropertyName = "anonymity")]
            public bool Anonymity { get; set; }
        }

        public class PopularityModel
        {
            [JsonProperty(PropertyName = "coefficient")]
            public int Coefficient { get; set; }

            [JsonProperty(PropertyName = "ranking")]
            public int Ranking { get; set; }
        }

        public class PropertyModel
        {
            [JsonProperty(PropertyName = "freeSpin")]
            public FreeSpin FreeSpin { get; set; }

            [JsonProperty(PropertyName = "terminal")]
            public List<string> Terminal { get; set; }                
        }

        public class FreeSpin
        {
            [JsonProperty(PropertyName = "support")]
            public bool Support { get; set; }

            [JsonProperty(PropertyName = "lines")]
            public SelectionModel Lines { get; set; }

            [JsonProperty(PropertyName = "denominations")]
            public SelectionModel Denominations { get; set; }

            [JsonProperty(PropertyName = "betValues")]
            public SelectionModel BetValues { get; set; }
        }

        public class SelectionModel
        {
            [JsonProperty(PropertyName = "selections")]
            public List<decimal> Selections { get; set; }

            [JsonProperty(PropertyName = "default")]
            public decimal? DefaultValue { get; set; }
        }

        public class PresentationModel
        {
            [JsonProperty(PropertyName = "gameName")]
            public Item GameName { get; set; }

            [JsonProperty(PropertyName = "tableName")]
            public Item TableName { get; set; }

            [JsonProperty(PropertyName = "thumbnail")]
            public Item Thumbnail { get; set; }

            [JsonProperty(PropertyName = "backgroundImage")]
            public Item BackgroundImage { get; set; }
        }

        public class Item
        {
            [JsonProperty(PropertyName = "*")]
            public string ItemKey { get; set; }
        }
    }
}