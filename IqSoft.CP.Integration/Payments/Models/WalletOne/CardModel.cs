using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.WalletOne
{
    public class CardModel
    {
        [JsonProperty(PropertyName = "CardHolderName")]
        public string CardHolderName { get; set; }

        [JsonProperty(PropertyName = "CardNumber")]
        public string CardNumber { get; set; }

        [JsonProperty(PropertyName = "ExpiryDate")]
        public string ExpiryDate { get; set; }

        [JsonProperty(PropertyName = "Amount")]
        public string Amount { get; set; }
    }
}