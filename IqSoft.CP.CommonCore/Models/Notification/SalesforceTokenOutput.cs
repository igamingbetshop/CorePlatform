using Newtonsoft.Json;

namespace IqSoft.CP.Common.Models
{
    public class SalesforceTokenOutput
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "rest_instance_url")]
        public string RestInstanceUrl { get; set; }
    }
}
