using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models
{
    public class PurgeOutput
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "messages")]
        public string[] Messages { get; set; }

        [JsonProperty(PropertyName = "result")]
        public Result ResultData { get; set; }
    }

    public class Result
    {
        [JsonProperty(PropertyName = "id")]
        public string ZoneId{ get; set; }
    }
    }
