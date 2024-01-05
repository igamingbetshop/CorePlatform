using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Pix
{
    public class WithdrawResponse
    {
        [JsonProperty(PropertyName = "uuid")]
        public string UuId { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "recipient_document")]
        public string RecipientDocument { get; set; }

        [JsonProperty(PropertyName = "recipient_name")]
        public string RecipientName { get; set; }

        [JsonProperty(PropertyName = "address_key")]
        public string AddressKey { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public double Amount { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
}
