using GraphQL;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.AleaPlay
{
    public class GamesOutput
    {
        [GraphQLMetadata("gamesReadyToPlay")]
        public List<Game> GamesReadyToPlay { get; set; }
    }

    public class Game
    {
        [GraphQLMetadata("id")]
        public string Id { get; set; }

        [GraphQLMetadata("name")]
        public string Name { get; set; }

        [GraphQLMetadata("software")]
        public Software Software { get; set; }

        [GraphQLMetadata("categories")]
        public List<Category> Categories { get; set; }

        [GraphQLMetadata("configurations")]
        public List<Configuration> Configurations { get; set; }

        [GraphQLMetadata("fsConfig")]
        public List<Fsconfig> FsConfig { get; set; }
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

    public class Fsconfig
    {
        public string Currency { get; set; }
        public Costsperlevel[] CostsPerLevel { get; set; }
    }

    public class Costsperlevel
    {
        public int? Level { get; set; }
        public float? Cost { get; set; }
    }

}


