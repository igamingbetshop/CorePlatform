using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.GumballPay
{
	public class PaymentOutput
	{
		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }

		[JsonProperty(PropertyName = "paynet-order-id")]
		public string PaynetOrderId { get; set; }

		[JsonProperty(PropertyName = "merchant-order-id")]
		public string MerchantOrderId { get; set; }

		[JsonProperty(PropertyName = "serial-number")]
		public string SerialNumber { get; set; }

		[JsonProperty(PropertyName = "error-message")]
		public string ErrorMessage { get; set; }

		[JsonProperty(PropertyName = "error-code")]
		public string ErrorCode { get; set; }

		[JsonProperty(PropertyName = "redirect-url")]
		public string RedirectUrl { get; set; }
    }
}
