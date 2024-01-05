using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Platforms.Models
{
    public class CloudOutput
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "messages")]
        public string[] Messages { get; set; }

        [JsonProperty(PropertyName = "result")]
        public object ResultData { get; set; }
    }

    public class PurgeResult
    {
        [JsonProperty(PropertyName = "id")]
        public string ZoneId { get; set; }
    }

    public class DnsItem
    {
        public string RowId { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "zone_id")]
        public string ZoneId { get; set; }

        [JsonProperty(PropertyName = "zone_name")]
        public string ZoneName { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }

        [JsonProperty(PropertyName = "proxiable")]
        public bool Proxiable { get; set; }

        [JsonProperty(PropertyName = "proxied")]
        public bool Proxied { get; set; }

        [JsonProperty(PropertyName = "ttl")]
        public int TTL { get; set; }

        [JsonProperty(PropertyName = "locked")]
        public bool Locked { get; set; }

        [JsonProperty(PropertyName = "comment")]
        public string Comment { get; set; }

        [JsonProperty(PropertyName = "created_on")]
        public DateTime CreatedOn { get; set; }

        [JsonProperty(PropertyName = "modified_on")]
        public DateTime ModifiedOn { get; set; }

        [JsonProperty(PropertyName = "meta")]
        public Meta Meta { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public List<Tag> Tags { get; set; }

        [JsonProperty(PropertyName = "priority")]
        public int? Priority { get; set; }
    }

    public class Meta
    {
        [JsonProperty(PropertyName = "auto_added")]
        public bool AutoAdded { get; set; }

        [JsonProperty(PropertyName = "managed_by_apps")]
        public bool ManagedByApps { get; set; }

        [JsonProperty(PropertyName = "managed_by_argo_tunnel")]
        public bool ManagedByArgoTunnel { get; set; }

        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }
    }

    public class Tag
    {

    }
}
