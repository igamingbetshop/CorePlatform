using Newtonsoft.Json;
namespace IqSoft.CP.ProductGateway.Models.AleaPartners
{
    public class AuthenticationOutput : TransactionResult
    {
        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "userUuid")]
        public string UserUuid { get; set; }

        [JsonProperty(PropertyName = "ccy")]
        public string CurrencyId { get; set; }

        [JsonProperty(PropertyName = "language")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "country")] 
        public string Country { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "uexternalIdserUuid")]
        public string ExternalId { get; set; }

        [JsonProperty(PropertyName = "location")]
        public int Location { get; set; }

        [JsonProperty(PropertyName = "lotCode1")]
        public string LotCode1 { get; set; }

        [JsonProperty(PropertyName = "lotCode2")]
        public object LotCode2 { get; set; }

        [JsonProperty(PropertyName = "locationAddress")]
        public string LocationAddress { get; set; }

        [JsonProperty(PropertyName = "userCity")]
        public string UserCity { get; set; }
    }

}