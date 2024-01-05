using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.SantimaPay
{
	public class PaymentOutput
	{
		[JsonProperty(PropertyName = "url")]
		public string Url { get; set; }

		[JsonProperty(PropertyName = "reason")]
		public string Reason { get; set; }
	}
}
