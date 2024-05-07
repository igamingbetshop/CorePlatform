using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.RiseUp
{
	public class TransactionInput : BaseInput
	{
		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }

		[JsonProperty(PropertyName = "bet")]
		public decimal BetAmount { get; set; }

		[JsonProperty(PropertyName = "win")]
		public decimal WinAmount { get; set; }

		[JsonProperty(PropertyName = "amount")]
		public decimal TotalAmount { get; set; }

		[JsonProperty(PropertyName = "rid")]
		public string RoundId { get; set; }

		[JsonProperty(PropertyName = "tid")]
		public string TransactionId { get; set; }
	}
}