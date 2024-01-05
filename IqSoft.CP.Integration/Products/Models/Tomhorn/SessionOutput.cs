using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Products.Models.Tomhorn
{
    public class SessionOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "Session")]
        public SessionType Session { get; set; }
    }

    public class SessionType
    {
        [JsonProperty(PropertyName = "End")]
        public DateTime? EndDate { get; set; }

        [JsonProperty(PropertyName = "ID")]
        public long Id { get; set; }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Start")]
        public DateTime StartDate { get; set; }

        [JsonProperty(PropertyName = "State")]
        public string State { get; set; }
    }
}
