using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models
{
    public class CustomerInput
    {

        [JsonProperty(PropertyName = "to")]
        public string ToEmail { get; set; }

        [JsonProperty(PropertyName = "from")]
        public string FromEmail { get; set; }

        [JsonProperty(PropertyName = "subject")]
        public string Subject { get; set; }

        [JsonProperty(PropertyName = "body")]
        public string Body { get; set; }
        [JsonProperty(PropertyName = "identifiers")]
        public Dictionary<string, string> Identifiers { get; set; }

        [JsonProperty(PropertyName = "attachments")]
        public Dictionary<string, string> Attachments { get; set; }
    }
}
