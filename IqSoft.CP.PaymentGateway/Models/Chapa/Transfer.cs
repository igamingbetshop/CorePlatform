using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Chapa
{
	public class Transfer
	{
		[JsonProperty(PropertyName = "account_name")]
		public string AccountName { get; set; }

		[JsonProperty(PropertyName = "account_number")]
		public string AccountNumber { get; set; }

		[JsonProperty(PropertyName = "bank_id")]
		public string BankId { get; set; }

		[JsonProperty(PropertyName = "bank_name")]
		public string BankName { get; set; }

		[JsonProperty(PropertyName = "currency")]
		public string Currency { get; set; }

		[JsonProperty(PropertyName = "amount")]
		public string Amount { get; set; }

		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }

		[JsonProperty(PropertyName = "status")]
		public string Status { get; set; }

		[JsonProperty(PropertyName = "reference")]
		public string Reference { get; set; }

		[JsonProperty(PropertyName = "chapa_reference")]
		public string ChapaReference { get; set; }
	}
}