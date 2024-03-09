using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Jmitsolutions
{
	public class PaymentInput
	{
		[JsonProperty(PropertyName = "token")]
		public string Token { get; set; }

		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }

		[JsonProperty(PropertyName = "status")]
		public string Status { get; set; }

		[JsonProperty(PropertyName = "orderNumber")]
		public string OrderNumber { get; set; }

		[JsonProperty(PropertyName = "amount")]
		public int Amount { get; set; }

		[JsonProperty(PropertyName = "currency")]
		public string Currency { get; set; }

		[JsonProperty(PropertyName = "gatewayDetails")]
		public GatewayDetails GatewayDetails { get; set; }
	}
	public class GatewayDetails
	{
		[JsonProperty(PropertyName = "decline_reason")]
		public string DeclineReason { get; set; }
	}
}