using GraphQL;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.AleaPlay
{
    public class GamesOutput
    {
        [GraphQLMetadata("gamesAvailable")]
        public GamesAvailable GamesAvailable { get; set; }

        [GraphQLMetadata("gamesReady")]
        public GamesAvailable GamesReady { get; set; }
    }

    public class GamesAvailable
    {
        [GraphQLMetadata("page")]
        public Page Page { get; set; }

        [GraphQLMetadata("results")]
        public List<Game> Results { get; set; }
    }

    public class Page
    {
        [GraphQLMetadata("number")]
        public int Number { get; set; }
        
        [GraphQLMetadata("size")]
        public int Size { get; set; }
        
        [GraphQLMetadata("totalPages")]
        public int TotalPages { get; set; }

        [GraphQLMetadata("totalElements")]
        public int TotalElements { get; set; }
    }

    public class Game
    {
        [GraphQLMetadata("id")]
        public string Id { get; set; }

        [GraphQLMetadata("name")]
        public string Name { get; set; }

        [GraphQLMetadata("software")]
        public Software Software { get; set; }

        [GraphQLMetadata("genre")]
        public string Genre { get; set; }
        
        [GraphQLMetadata("rtp")]
        public decimal? RTP { get; set; }
        
        [GraphQLMetadata("volatility")]
        public string Volatility { get; set; }

        [GraphQLMetadata("lines")]
        public string Lines { get; set; }

        [GraphQLMetadata("thumbnailLinks")]
        public Thumbnail ThumbnailLinks { get; set; }

        [GraphQLMetadata("freeSpinsCurrencies")]
        public List<string> FreeSpinsCurrencies { get; set; }

    }

    public class Thumbnail
    {
        [GraphQLMetadata("RATIO_4_3")]
        public string RATIO_4_3 { get; set; }
       [GraphQLMetadata("RATIO_3_4")]
        public string RATIO_3_4 { get; set; }
    }

    public class Software
    {
        [GraphQLMetadata("id")]
        public string Id { get; set; }

        [GraphQLMetadata("name")]
        public string Name { get; set; }
    }

    public class Category
    {
        [GraphQLMetadata("id")]
        public string Id { get; set; }

        [GraphQLMetadata("name")]
        public string Name { get; set; }
    }

    public class Configuration
    {
        [GraphQLMetadata("id")]
        public string Id { get; set; }

        [GraphQLMetadata("device")]
        public string Device { get; set; }

        [GraphQLMetadata("attributes")]
        public Attribute[] Attributes { get; set; }
    }

    public class Attribute
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }
}


