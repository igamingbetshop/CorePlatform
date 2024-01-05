using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Elite
{
	public class TransactionInput
	{
		[JsonProperty(PropertyName = "transactionId")]
		public string TransactionId { get; set; }

		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }

		[JsonProperty(PropertyName = "amount")]
		public decimal Amount { get; set; }

		[JsonProperty(PropertyName = "adminDescription")]
		public string AdminDescription { get; set; }

		[JsonProperty(PropertyName = "status")]
		public string Status { get; set; }
	}
}