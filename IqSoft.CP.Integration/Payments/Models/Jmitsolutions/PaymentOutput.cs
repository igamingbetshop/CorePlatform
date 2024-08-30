using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Jmitsolutions
{
	public class PaymentOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "processingUrl")]
        public string ProcessingUrl { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }
	}
}
