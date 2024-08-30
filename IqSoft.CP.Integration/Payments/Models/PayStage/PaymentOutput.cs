using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.PayStage
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "data")]
        public DataModel Data { get; set; }

        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }
    }

    public class DataModel
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "transaction_number")]
        public string TransactionNumber { get; set; }

        [JsonProperty(PropertyName = "checkout_url")]
        public string CheckoutUrl { get; set; }
    }
}
