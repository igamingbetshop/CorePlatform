using Newtonsoft.Json;
namespace IqSoft.CP.Integration.Platforms.Models.OASIS
{
    public class PlayerInput : BaseInput
    {
        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "birth_date")]
        public string BirthDate { get; set; }
    }
}