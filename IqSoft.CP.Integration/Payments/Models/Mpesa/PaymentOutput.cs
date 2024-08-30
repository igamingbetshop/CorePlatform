using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Mpesa
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "response")]
        public ResponseModel Response { get; set; }
    }

    public class ResponseModel
    {
        public string MerchantRequestID { get; set; }
        public int ResponseCode { get; set; }
        public string CustomerMessage { get; set; }
        public string CheckoutRequestID { get; set; }
        public string ResponseDescription { get; set; }
    }
}
