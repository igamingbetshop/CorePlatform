using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Pay3000
{
	public class ConsentInput
	{
		[JsonProperty(PropertyName = "consentId")]
		public string ConsentId { get; set; }

		[JsonProperty(PropertyName = "status")]
		public string Status { get; set; }
	}
}