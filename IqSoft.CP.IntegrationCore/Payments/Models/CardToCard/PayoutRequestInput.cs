using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.CardToCard
{
    public  class PayoutRequestInput
    {
        [JsonProperty(PropertyName = "amount")]
        public int Amount { get; set; }

        [JsonProperty(PropertyName = "ref_id")]
        public long PaymentId { get; set; }

        [JsonProperty(PropertyName = "pan")]
        public string BankCardNumber { get; set; }

        [JsonProperty(PropertyName = "sheba")]
        public string BankACH { get; set; }

        [JsonProperty(PropertyName = "holder_nam")]
        public string BankAccountHolder { get; set; }

        [JsonProperty(PropertyName = "verify_hash")]
        public string Hash { get; set; }
    }
}
