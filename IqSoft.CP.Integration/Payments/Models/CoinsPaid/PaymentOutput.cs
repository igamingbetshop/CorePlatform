using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.CoinsPaid
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "data")]
        public PaymentData Data { get; set; }
    }

    public class PaymentData
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "foreign_id")]
        public string ForeignId { get; set; }

        [JsonProperty(PropertyName = "convert_to")]
        public string Convert_to { get; set; }

        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "tag")]
        public string Tag { get; set; }
    }
    }
