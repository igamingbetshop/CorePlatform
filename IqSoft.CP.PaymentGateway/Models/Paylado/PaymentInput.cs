using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Paylado
{
	public class PaymentInput
	{
		[JsonProperty(PropertyName = "ResultStatus")]
		public string ResultStatus { get; set; }

		[JsonProperty(PropertyName = "ResultCode")]
		public string ResultCode { get; set; }

		[JsonProperty(PropertyName = "ResultMessage")]
		public string ResultMessage { get; set; }

		[JsonProperty(PropertyName = "TransactionId")]
		public string TransactionId { get; set; }

		[JsonProperty(PropertyName = "TransactionType")]
		public string TransactionType { get; set; }

		[JsonProperty(PropertyName = "TransactionReference")]
		public string TransactionReference { get; set; }

		[JsonProperty(PropertyName = "TransactionDate")]
		public string TransactionDate { get; set; }

		[JsonProperty(PropertyName = "Amount")]
		public string Amount { get; set; }

		[JsonProperty(PropertyName = "Currency")]
		public string Currency { get; set; }

		[JsonProperty(PropertyName = "TransactionUrl")]
		public string TransactionUrl { get; set; }

		[JsonProperty(PropertyName = "SettlementDate")]
		public string SettlementDate { get; set; }

		[JsonProperty(PropertyName = "IpAddress")]
		public string IpAddress { get; set; }

		[JsonProperty(PropertyName = "FirstName")]
		public string FirstName { get; set; }

		[JsonProperty(PropertyName = "LastName")]
		public string LastName { get; set; }

		[JsonProperty(PropertyName = "Email")]
		public string Email { get; set; }

		[JsonProperty(PropertyName = "PaymentType")]
		public string PaymentType { get; set; }

		[JsonProperty(PropertyName = "PaymentOptionAlias")]
		public string PaymentOptionAlias { get; set; }

		[JsonProperty(PropertyName = "AccountHolder")]
		public string AccountHolder { get; set; }

		[JsonProperty(PropertyName = "PayladoId")]
		public string PayladoId { get; set; }

		[JsonProperty(PropertyName = "Token")]
		public string Token { get; set; }

		[JsonProperty(PropertyName = "HasChargeback")]
		public string HasChargeback { get; set; }

		[JsonProperty(PropertyName = "MaxAttempts")]
		public string MaxAttempts { get; set; }

		[JsonProperty(PropertyName = "Attempt")]
		public string Attempt { get; set; }
	}
}