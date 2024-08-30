using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace IqSoft.CP.Integration.Payments.Models.Omno
{
    public class AuthOutput
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty(PropertyName = "refresh_expires_in")]
        public int RefreshExpiresIn { get; set; }

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }

        [JsonProperty(PropertyName = "not-before-policy")]
        public int NotBeforePolicy { get; set; }

        [JsonProperty(PropertyName = "scope")]
        public string Scope { get; set; }
    }

    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "paymentId")]
        public string PaymentId { get; set; }

        [JsonProperty(PropertyName = "paymentUrl")]
        public string PaymentUrl { get; set; }

        [JsonProperty(PropertyName = "paymentUrlIframe")]
        public string PaymentUrlIFrame { get; set; }

        [JsonProperty(PropertyName = "paymentUrlIframeApm")]
        public string PaymentUrlIFrameApm { get; set; }
    }
}
