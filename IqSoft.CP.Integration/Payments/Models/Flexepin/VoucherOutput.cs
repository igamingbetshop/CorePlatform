using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Flexepin
{
    public class VoucherOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "transaction_id")]
        public string Transaction_id { get; set; }

        [JsonProperty(PropertyName = "trans_no")]
        public string TransNo { get; set; }

        [JsonProperty(PropertyName = "serial")]
        public string Serial { get; set; }

        [JsonProperty(PropertyName = "value")]
        public int Value { get; set; }

        [JsonProperty(PropertyName = "cost")]
        public decimal Cost { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "ean")]
        public string Ean { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
}
