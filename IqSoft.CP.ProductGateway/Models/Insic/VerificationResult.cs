using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Insic
{
    public class VerificationResult
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "createdAt")]
        public string CreatedAt { get; set; }

        [JsonProperty(PropertyName = "updatedAt")]
        public string UpdatedAt { get; set; }

        [JsonProperty(PropertyName = "partnerId")]
        public string PartnerId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int? Status { get; set; }

        [JsonProperty(PropertyName = "role")]
        public string Role { get; set; }

        [JsonProperty(PropertyName = "isBlocked")]
        public bool IsBlocked { get; set; }

        [JsonProperty(PropertyName = "lang")]
        public string Lang { get; set; }

        [JsonProperty(PropertyName = "groupId")]
        public string GroupId { get; set; }

        [JsonProperty(PropertyName = "credentialsChangedAt")]
        public string CredentialsChangedAt { get; set; }

        [JsonProperty(PropertyName = "shopId")]
        public string ShopId { get; set; }

        [JsonProperty(PropertyName = "smsAuth")]
        public string SMSAuth { get; set; }

        [JsonProperty(PropertyName = "persKey")]
        public string PersKey { get; set; }

        [JsonProperty(PropertyName = "profile")]
        public UserProfile Profile { get; set; }

        [JsonProperty(PropertyName = "serviceName")]
        public string ServiceName { get; set; }

        [JsonProperty(PropertyName = "serviceStatus")]
        public int ServiceStatus { get; set; }
    }

    public class UserProfile
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "greeting")]
        public string greeting { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "birthday")]
        public string BirthDay { get; set; }

        [JsonProperty(PropertyName = "zip")]
        public string ZipCode { get; set; }

        [JsonProperty(PropertyName = "province")]
        public string Province { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "street")]
        public string Street { get; set; }

        [JsonProperty(PropertyName = "houseNumber")]
        public string HouseNumber { get; set; }

        [JsonProperty(PropertyName = "phone")]
        public string Phone { get; set; }

        [JsonProperty(PropertyName = "iban")]
        public string IBAN { get; set; }

        [JsonProperty(PropertyName = "bic")]
        public string Bic { get; set; }

        [JsonProperty(PropertyName = "agreement1")]
        public string Agreement1 { get; set; }

        [JsonProperty(PropertyName = "agreement2")]
        public string Agreement2 { get; set; }

        [JsonProperty(PropertyName = "placeOfBirth")]
        public string PlaceOfBirth { get; set; }

        [JsonProperty(PropertyName = "passportFrontId")]
        public string PassportFrontId { get; set; }

        [JsonProperty(PropertyName = "passportBackId")]
        public string PassportBackId { get; set; }

        [JsonProperty(PropertyName = "imageId")]
        public string ImageId { get; set; }

        [JsonProperty(PropertyName = "nationality")]
        public string Nationality { get; set; }

        [JsonProperty(PropertyName = "maidenName")]
        public string MaidenName { get; set; }

        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }
    }
}