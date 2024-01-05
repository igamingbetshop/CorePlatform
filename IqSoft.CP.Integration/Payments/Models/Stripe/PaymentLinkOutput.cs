using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Stripe
{
	public class PaymentLinkOutput
	{
		[JsonProperty(PropertyName = "url")]
		public string Url { get; set; }
	}
}
