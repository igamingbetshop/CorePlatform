using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Azulpay
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }
    }

    public class Data
    {
        [JsonProperty(PropertyName = "descriptor")]
        public string Descriptor { get; set; }

        [JsonProperty(PropertyName = "gatewayResponse")]
        public string GatewayResponse { get; set; }

        [JsonProperty(PropertyName = "paymentId")]
        public string PaymentId { get; set; }

        [JsonProperty(PropertyName = "orderid")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "gatewayStatus")]
        public string GatewayStatus { get; set; }

        [JsonProperty(PropertyName = "redirectUrl")]
        public string RedirectUrl { get; set; }
    }

}
