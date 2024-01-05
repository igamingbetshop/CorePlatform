using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Ngine
{
    public class AuthenticationInput
    {
        [JsonProperty(PropertyName = "userLogin")]
        public string UserLogin { get; set; }

        [JsonProperty(PropertyName = "userPassword")]
        public string UserPassword { get; set; }

        [JsonProperty(PropertyName = "InstanceID")]
        public int InstanceID { get; set; }
    }
}
