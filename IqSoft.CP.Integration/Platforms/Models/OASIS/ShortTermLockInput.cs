using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.OASIS
{
    public class ShortTermLockInput : PlayerInput
    {

        [JsonProperty(PropertyName = "birth_name")]
        public string BirthName { get; set; }

        [JsonProperty(PropertyName = "birth_place")]
        public string BirthPlace { get; set; }

        [JsonProperty(PropertyName = "addr_zip")]
        public string ZipCode { get; set; }

        [JsonProperty(PropertyName = "addr_city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "addr_street")]
        public string Street { get; set; }

        [JsonProperty(PropertyName = "addr_houseno")]
        public string HouseNumber { get; set; }

        [JsonProperty(PropertyName = "addr_country")]
        public string CountryCode { get; set; }
    }
}
