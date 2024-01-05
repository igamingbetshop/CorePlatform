using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Platforms.Models.Webflow
{
    public class CollectionOutput
    {
        [JsonProperty(PropertyName = "items")]
        public List<BaseOutput> Items { get; set; }
    }
    public class ItemOutput
    {
        [JsonProperty(PropertyName = "items")]
        public List<ItemModel> Items { get; set; }
    }
    public class ItemModel
    {
        [JsonProperty(PropertyName = "post-body")]
        public string PostBody { get; set; }
    }
}
