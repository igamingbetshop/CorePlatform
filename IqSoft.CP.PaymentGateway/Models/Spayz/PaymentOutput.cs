using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Spayz
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; } = "success";
    }
}