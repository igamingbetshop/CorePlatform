using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Models.IXOPay
{
    public class PayoutOutput
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "uuid")]
        public string Uuid { get; set; }

        [JsonProperty(PropertyName = "purchaseId")]
        public string PurchaseId { get; set; }

        [JsonProperty(PropertyName = "returnType")]
        public string ReturnType { get; set; }

        [JsonProperty(PropertyName = "paymentMethod")]
        public string PaymentMethod { get; set; }

        [JsonProperty(PropertyName = "errors")]
        public List<PayoutError> Errors { get; set; }
    }
    public class PayoutError
    {

        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty(PropertyName = "errorCode")]
        public int ErrorCode { get; set; }

        [JsonProperty(PropertyName = "adapterMessage")]
        public string AdapterMessage { get; set; }

        [JsonProperty(PropertyName = "adapterCode")]
        public string AdapterCode { get; set; }
    }
}
