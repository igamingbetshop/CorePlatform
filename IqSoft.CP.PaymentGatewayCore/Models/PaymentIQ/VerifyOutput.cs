using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.PaymentIQ
{
    public class VerifyOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "userCat")]
        public string UserCat { get; set; }

        [JsonProperty(PropertyName = "kycStatus")]
        public string KycStatus { get; set; }

        [JsonProperty(PropertyName = "sex")]
        public string Gender { get; set; }

        [JsonProperty(PropertyName = "street")]
        public string Street { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "zip")]
        public string Zip { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "dob")]
        public string Dob { get; set; }

        [JsonProperty(PropertyName = "mobile")]
        public string MobileNumber { get; set; }

        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "balanceCy")]
        public string UserCurrency { get; set; }

        [JsonProperty(PropertyName = "attributes")]
        public AttributeModel Attributes { get; set; }
    }
}