using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Runpay
{
    public class PayoutOutput
    {
        [JsonProperty(PropertyName = "clientTranId")]
        public string ClientTranId { get; set; }

        [JsonProperty(PropertyName = "serverTranId")]
        public int ServerTranId { get; set; }

        [JsonProperty(PropertyName = "account")]
        public string Account { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "operatorCode")]
        public int OperatorCode { get; set; }

        [JsonProperty(PropertyName = "paymentType")]
        public int PaymentType { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "errorCode")]
        public int ErrorCode { get; set; }

        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; }
    }
}
