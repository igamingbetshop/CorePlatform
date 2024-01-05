using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.SerosPay
{
  public  class LoginInput
    {
        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
    }
}
