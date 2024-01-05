using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.EveryMatrix
{
    public class GameItem
    {
        public int domainID { get; set; }
        public string type { get; set; }
        public string action { get; set; }
        public string id { get; set; }
        public int seq { get; set; }
        public Data data { get; set; }
    }

    public class Data
    {
        public string id { get; set; }
        public string slug { get; set; }
        public string vendor { get; set; }
        public int vendorID { get; set; }
        public string vendorDisplayName { get; set; }
        public string contentProvider { get; set; }
        public string originalVendor { get; set; }
        public string gameCode { get; set; }
        public string gameBundleID { get; set; }
        public string gameID { get; set; }
        public bool enabled { get; set; }
        public string url { get; set; }
        public List<string> categories { get; set; }
        public List<string> restrictedTerritories { get; set; }
        public decimal? theoreticalPayOut { get; set; }
        public PlayMode playMode { get; set; }
        public Presentation presentation { get; set; }
        public Property property { get; set; }

        public class PlayMode
        {
            public bool fun { get; set; }
            public bool realMoney { get; set; }
            public bool anonymity { get; set; }
        }

        public class Popularity
        {
            public int coefficient { get; set; }
            public int ranking { get; set; }
        }

        public class Property
        {
            public FreeSpin freeSpin { get; set; }                
            public List<string> terminal { get; set; }                
        }

        public class FreeSpin
        {
            public bool support { get; set; }
        }

        public class Presentation
        {
            public Item gameName { get; set; }
            public Item tableName { get; set; }
            public Item thumbnail { get; set; }
            public Item backgroundImage { get; set; }
        }

        public class Item
        {
            [JsonProperty(PropertyName = "*")]
            public string itemKey { get; set; }
        }
    }
}