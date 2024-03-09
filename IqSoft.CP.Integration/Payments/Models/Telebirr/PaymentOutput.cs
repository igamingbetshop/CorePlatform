using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Telebirr
{
	public class PaymentOutput
	{
		[JsonProperty(PropertyName = "code")]
		public int Code { get; set; }

		[JsonProperty(PropertyName = "message")]
		public string Message { get; set; }

		[JsonProperty(PropertyName = "data")]
		public Data UrlData { get; set; }
		public class Data
		{
			[JsonProperty(PropertyName = "toPayUrl")]
			public string ToPayUrl { get; set; }
		}
	}
}
