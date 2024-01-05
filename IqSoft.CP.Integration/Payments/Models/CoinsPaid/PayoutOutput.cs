using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.CoinsPaid
{
    public class PayoutOutput
    {
        [JsonProperty(PropertyName = "data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "foreign_id")]
        public string ForeignId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "sender_amount")]
        public string SenderAmount { get; set; }

        [JsonProperty(PropertyName = "sender_currency")]
        public string SenderCurrency { get; set; }

        [JsonProperty(PropertyName = "receiver_amount")]
        public string ReceiverAmount { get; set; }

        [JsonProperty(PropertyName = "receiver_currency")]
        public string ReceiverCurrency { get; set; }
    }
}
