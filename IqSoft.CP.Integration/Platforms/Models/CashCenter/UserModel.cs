using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.CashCenter
{
    public class UserModel
    {
        [JsonProperty(PropertyName = "sub")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "currencyId")]
        public string CurrencyId { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }
    }
}
