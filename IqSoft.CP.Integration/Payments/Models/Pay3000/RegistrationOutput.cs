using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Models.Pay3000
{
	public class RegistrationOutput
    {
        [JsonProperty(PropertyName = "kycInfo")]
        public KycInfo KycInfo { get; set; }

        [JsonProperty(PropertyName = "customerId")]
        public string CustomerId { get; set; }

        [JsonProperty(PropertyName = "customerStatus")]
        public string CustomerStatus { get; set; }

        [JsonProperty(PropertyName = "viewOnly")]
        public bool ViewOnly { get; set; }
	}
    public class KycInfo
    {
        [JsonProperty(PropertyName = "kycLevel")]
        public int KycLevel { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "birthDate")]
        public string BirthDate { get; set; }

        [JsonProperty(PropertyName = "nationality")]
        public string Nationality { get; set; }

        [JsonProperty(PropertyName = "residenceCountry")]
        public string ResidenceCountry { get; set; }

        [JsonProperty(PropertyName = "documents")]
        public List<object> Documents { get; set; }
    }
}
