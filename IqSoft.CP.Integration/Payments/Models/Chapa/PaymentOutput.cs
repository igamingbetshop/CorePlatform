using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Chapa
{
	public class PaymentOutput
	{
		[JsonProperty(PropertyName = "status")]
		public string Status { get; set; }

		[JsonProperty(PropertyName = "message")]
		public string Message { get; set; }

		[JsonProperty(PropertyName = "data")]
		public object Data { get; set; }
	}

	public class Data
	{
		[JsonProperty(PropertyName = "checkout_url")]
		public string CheckoutUrl { get; set; }
	}
}
