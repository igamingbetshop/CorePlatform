using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Telebirr
{
	public class PaymentInput
	{
		[JsonProperty(PropertyName = "appId")]
		public string appId { get; set; }

		[JsonProperty(PropertyName = "appKey")]
		public string appKey { get; set; }

		[JsonProperty(PropertyName = "nonce")]
		public string nonce { get; set; }

		[JsonProperty(PropertyName = "notifyUrl")]
		public string notifyUrl { get; set; }

		[JsonProperty(PropertyName = "outTradeNo")]
		public string outTradeNo { get; set; }

		[JsonProperty(PropertyName = "returnUrl")]
		public string returnUrl { get; set; }

		[JsonProperty(PropertyName = "shortCode")]
		public string shortCode { get; set; }

		[JsonProperty(PropertyName = "subject")]
		public string subject { get; set; }

		[JsonProperty(PropertyName = "timeoutExpress")]
		public string timeoutExpress { get; set; }

		[JsonProperty(PropertyName = "timestamp")]
		public string timestamp { get; set; }

		[JsonProperty(PropertyName = "totalAmount")]
		public string totalAmount { get; set; }

		[JsonProperty(PropertyName = "receiveName")]
		public string receiveName { get; set; }
	}
}
