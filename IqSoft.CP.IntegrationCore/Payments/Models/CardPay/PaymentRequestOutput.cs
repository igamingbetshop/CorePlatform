using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.CardPay
{
    public class PaymentRequestOutput
    {
        [JsonProperty(PropertyName = "redirect_url")]
        public string RedirectUrl { get;set;}
    }
}
