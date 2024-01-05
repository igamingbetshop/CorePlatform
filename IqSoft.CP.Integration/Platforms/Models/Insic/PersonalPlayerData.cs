using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Platforms.Models.Insic
{
    public class PersonalPlayerData
    {
        [JsonProperty(PropertyName = "givenName")]
        public string GivenName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "street")]
        public string Street { get; set; }

        [JsonProperty(PropertyName = "number")]
        public string Number { get; set; }

        [JsonProperty(PropertyName = "postcode")]
        public string PostCode { get; set; }

        [JsonProperty(PropertyName = "place")]
        public string Place { get; set; }

        [JsonProperty(PropertyName = "area")]
        public string Area { get; set; }

        [JsonProperty(PropertyName = "countryAlpha2Code")]
        public string CountryAlpha2Code { get; set; }

        [JsonProperty(PropertyName = "birthDate")]
        public string BirthDate { get; set; }

        [JsonProperty(PropertyName = "birthPlace")]
        public string BirthPlace { get; set; }

        [JsonProperty(PropertyName = "birthName")]
        public string BirthName { get; set; }
    }
}
