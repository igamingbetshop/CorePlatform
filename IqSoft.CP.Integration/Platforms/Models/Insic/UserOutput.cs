using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.Insic
{
    public class UserOutput
    {
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "partnerId")]
        public string PartnerId { get; set; }

        [JsonProperty(PropertyName = "role")]
        public string Role { get; set; }

        [JsonProperty(PropertyName = "lang")]
        public string Langage { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "updatedAt")]
        public string UpdatedAt { get; set; }

        [JsonProperty(PropertyName = "createdAt")]
        public string CreatedAt { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "isBlocked")]
        public int IsBlocked { get; set; }

        [JsonProperty(PropertyName = "groupId")]
        public string GroupId { get; set; }

        [JsonProperty(PropertyName = "credentialsChangedAt")]
        public string credentialsChangedAt { get; set; }

        [JsonProperty(PropertyName = "shopId")]
        public string ShopId { get; set; }

        [JsonProperty(PropertyName = "smsAuth")]
        public string SMSAuth { get; set; }

        [JsonProperty(PropertyName = "persKey")]
        public string PersKey { get; set; }

        [JsonProperty(PropertyName = "profile")]
        public Profile ProfileDetails { get; set; }
    }

    public class Profile
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string userId { get; set; }

        [JsonProperty(PropertyName = "greeting")]
        public string Greeting { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "birthday")]
        public string Birthday { get; set; }

        [JsonProperty(PropertyName = "zip")]
        public string Zip { get; set; }

        [JsonProperty(PropertyName = "province")]
        public string Province { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string Sity { get; set; }

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

        [JsonProperty(PropertyName = "agreement3")]
        public string Agreement3 { get; set; }

        [JsonProperty(PropertyName = "agreement4")]
        public string Agreement4 { get; set; }

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
