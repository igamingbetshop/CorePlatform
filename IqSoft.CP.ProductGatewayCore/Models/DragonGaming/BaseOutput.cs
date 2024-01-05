using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.DragonGaming
{
    public class BaseOutput : StatusOutput
    {

        [JsonProperty(PropertyName = "account_id")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public int? Balance { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "bonus_amount")]
        public int? BonusAmount { get; set; }

        [JsonProperty(PropertyName = "transaction_id")]
        public string TransactionId { get; set; }
    }
}