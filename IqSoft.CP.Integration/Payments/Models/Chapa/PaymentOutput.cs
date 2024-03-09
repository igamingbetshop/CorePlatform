using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Chapa
{
	public class PaymentOutput : BaseOutput
	{	
		[JsonProperty(PropertyName = "data")]
		public object Data { get; set; }
	}

	public class DataModel
	{
		[JsonProperty(PropertyName = "checkout_url")]
		public string CheckoutUrl { get; set; }
	}
}
