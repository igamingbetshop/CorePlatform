using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.QuikiPay
{
	public class PaymentOutput
	{
		[JsonProperty(PropertyName = "payment_url")]
		public string PaymentUrl { get; set; }
	}
}
