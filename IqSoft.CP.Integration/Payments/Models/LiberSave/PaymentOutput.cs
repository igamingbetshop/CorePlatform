using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.LiberSave
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "pay_url")]
        public string RedirectUrl { get; set; }
    }
}
