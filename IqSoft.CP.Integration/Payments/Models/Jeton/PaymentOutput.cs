using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Jeton
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "paymentId")]
        public string PaymentId { get; set; }

        [JsonProperty(PropertyName = "orderId")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "checkout")]
        public string Checkout { get; set; }

        [JsonProperty(PropertyName = "qr")]
        public string QR { get; set; }

        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }
    }
}
