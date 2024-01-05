using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.MaldoPay
{
    public class PayoutInput
    {
        [JsonProperty(PropertyName = "transaction")]
        public PaymentTransaction Transaction { get; set; }
    }
    public class PaymentTransaction
    {
        [JsonProperty(PropertyName = "codeId")]
        public int CodeId { get; set; }

        [JsonProperty(PropertyName = "codeMessage")]
        public string CodeMessage { get; set; }

        [JsonProperty(PropertyName = "redirect")]
        public string Redirect { get; set; }
    }
}
