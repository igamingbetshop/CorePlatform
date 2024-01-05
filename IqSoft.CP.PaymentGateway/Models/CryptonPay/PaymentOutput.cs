using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.CryptonPay
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "transactionURL")]
        public string TransactionUrl { get; set; }
    }
}