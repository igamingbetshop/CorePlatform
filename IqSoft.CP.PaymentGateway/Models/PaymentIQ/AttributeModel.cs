using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.PaymentIQ
{
    public class AttributeModel
    {
        [JsonProperty(PropertyName = "merchantTxId")]
        public long MerchantTransactionId { get; set; }

        [JsonProperty(PropertyName = "sessionId")]
        public string SessionId { get; set; }

        [JsonProperty(PropertyName = "bonusCode")]
        public int? BonusCode { get; set; }

        [JsonProperty(PropertyName = "promoCode")]
        public string PromoCode { get; set; }
    }
}