using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.PaySec
{
    public class PaymentRequestInput
    {
        [JsonProperty(PropertyName = "header")]
        public InputHeader inputHeader { get; set; }

        [JsonProperty(PropertyName = "body")]
        public InputBody inputBody { get; set; }
    }

    public class InputHeader
    {
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "merchantCode")]
        public string MerchantCode { get; set; }

        [JsonProperty(PropertyName = "signature")]
        public string Signature { get; set; }
    }

    public class InputBody
    {
        [JsonProperty(PropertyName = "channelCode")]
        public string ChannelCode { get; set; }

        [JsonProperty(PropertyName = "bankCode")]
        public string BankCode { get; set; }

        [JsonProperty(PropertyName = "notifyURL")]
        public string NotifyURL { get; set; }

        [JsonProperty(PropertyName = "returnURL")]
        public string ReturnURL { get; set; }

        [JsonProperty(PropertyName = "orderAmount")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "orderTime")]
        public string OrderTime { get; set; }

        [JsonProperty(PropertyName = "cartId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string CurrencyId { get; set; }
    }
}