using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.GreenID
{
    public class VerificationResultOutput
    {
        [JsonProperty(PropertyName = "error")]
        public bool Error { get; set; }

        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty(PropertyName = "verificationResult")]
        public string VerificationResult { get; set; }

        [JsonProperty(PropertyName = "verificationId")]
        public string VerificationId { get; set; }

        [JsonProperty(PropertyName = "verificationToken")]
        public string VerificationToken { get; set; }

        [JsonProperty(PropertyName = "givenName")]
        public string GivenName { get; set; }

        [JsonProperty(PropertyName = "middleNames")]
        public string MiddleNames { get; set; }

        [JsonProperty(PropertyName = "surname")]
        public string Surname { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "dob")]
        public string Bob { get; set; }

        [JsonProperty(PropertyName = "flatNumber")]
        public string FlatNumber { get; set; }

        [JsonProperty(PropertyName = "streetNumber")]
        public string StreetNumber { get; set; }

        [JsonProperty(PropertyName = "streetName")]
        public string StreetName { get; set; }

        [JsonProperty(PropertyName = "streetType")]
        public string StreetType { get; set; }

        [JsonProperty(PropertyName = "suburb")]
        public string Suburb { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "postcode")]
        public string Postcode { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }
    }
}
