using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.SerosPay
{
    public class UserLoginOutput
    {
        [JsonProperty(PropertyName = "username")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "activeProfile")]
        public string ActiveProfile { get; set; }

        [JsonProperty(PropertyName = "profiles")]
        public Profile[] Profiles { get; set; }
    }

    public class Profile
    {
        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "companyId")]
        public string CompanyId { get; set; }

        [JsonProperty(PropertyName = "companyName")]
        public string CompanyName { get; set; }

        [JsonProperty(PropertyName = "role")]
        public string Role { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "enabled")]
        public string Enabled { get; set; }
    }
}
