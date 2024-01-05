using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.JetonHavale
{
	public class PaymentInput
	{
		[JsonProperty(PropertyName = "transactionId")]
		public string TransactionId { get; set; }

		[JsonProperty(PropertyName = "status")]
		public string Status { get; set; }

		[JsonProperty(PropertyName = "amount")]
		public int Amount { get; set; }

		[JsonProperty(PropertyName = "hash")]
		public string Hash { get; set; }

		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }
	}
}