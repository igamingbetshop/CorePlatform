using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Models.IXOPay
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "purchaseId")]
        public string PurchaseId { get; set; }

        [JsonProperty(PropertyName = "returnType")]
        public string ReturnType { get; set; }

        [JsonProperty(PropertyName = "redirectUrl")]
        public string RedirectUrl { get; set; }

        [JsonProperty(PropertyName = "paymentMethod")]
        public string PaymentMethod { get; set; }

        [JsonProperty(PropertyName = "errors")]
        public List<Error> Error { get; set; }
    }

    public class Error
    {
        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty(PropertyName = "errorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "adapterMessage")]
        public string AdapterMessage { get; set; }

        [JsonProperty(PropertyName = "adapterCode")]
        public string AdapterCode { get; set; }
    }
}