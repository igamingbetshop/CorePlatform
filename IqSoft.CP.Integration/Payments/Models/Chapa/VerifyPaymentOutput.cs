using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Chapa
{
    public class VerifyPaymentOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "data")]
        public PaymentData Data { get; set; }
    }

    public class PaymentData
    {
        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "reference")]
        public string Reference { get; set; }

        [JsonProperty(PropertyName = "chapa_transfer_id")]
        public string ChapaTransferId { get; set; }

        [JsonProperty(PropertyName = "tx_ref")]
        public string TxRef { get; set; }
    }
}
