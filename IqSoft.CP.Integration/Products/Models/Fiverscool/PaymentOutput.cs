using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.Fiverscool
{
	public class PaymentOutput
	{
		[JsonProperty(PropertyName = "status")]
		public int Status { get; set; }

		[JsonProperty(PropertyName = "msg")]
		public string Message { get; set; }

		[JsonProperty(PropertyName = "fc_code")]
		public string FcCode { get; set; }

		[JsonProperty(PropertyName = "launch_url")]
		public string LaunchUrl { get; set; }
	}
}
