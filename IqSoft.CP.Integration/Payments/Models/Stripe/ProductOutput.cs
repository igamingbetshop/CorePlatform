using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Stripe
{
	internal class ProductOutput
	{
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }
	}
}
