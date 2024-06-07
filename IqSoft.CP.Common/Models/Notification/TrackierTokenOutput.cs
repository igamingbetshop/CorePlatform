using Newtonsoft.Json;

namespace IqSoft.CP.Common.Models.Notification
{
    public class TrackierTokenOutput
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "data")]
        public DataModel Data { get; set; }
    }

    public class DataModel
    {
        [JsonProperty(PropertyName = "accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "refreshToken")]
        public string RefreshToken { get; set; }
    }
}
