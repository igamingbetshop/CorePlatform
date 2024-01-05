using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.OASIS
{
    public class BaseInput
    {
        [JsonProperty(PropertyName = "auth_token")]
        public string AuthToken { get; set; }

        [JsonProperty(PropertyName = "request_type")]
        public string RequestType { get; set; }

        [JsonProperty(PropertyName = "branch_id")]
        public int BranchId { get; set; }
    }
}
