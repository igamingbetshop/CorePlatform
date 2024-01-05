using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Platforms.Models.Insic
{
    public class RequestBase
    {
        [JsonProperty(PropertyName = "eventName")]
        public string EventName { get; set; }

        [JsonProperty(PropertyName = "eventTime")]
        public string EventTime { get; set; }

        [JsonProperty(PropertyName = "playerId")]
        public string PlayerId { get; set; }

        [JsonProperty(PropertyName = "eventId")]
        public string EventId { get; set; }
    }
}
