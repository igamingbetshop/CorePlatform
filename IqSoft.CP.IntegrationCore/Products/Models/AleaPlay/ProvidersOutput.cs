using GraphQL;
using System.Collections.Generic;


namespace IqSoft.CP.Integration.Products.Models.AleaPlay
{
    public class ProvidersOutput
    {
        [GraphQLMetadata("software")]
        public SoftwareModel Software { get; set; }
    }

    public class SoftwareModel
    {
        [GraphQLMetadata("results")]
        public List<ProviderItem> Results { get; set; }
    }

    public class ProviderItem
    {
        [GraphQLMetadata("id")]
        public string Id { get; set; }

        [GraphQLMetadata("name")]
        public string Name { get; set; }
    }
}
