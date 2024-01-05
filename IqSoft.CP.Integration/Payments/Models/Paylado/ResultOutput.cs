namespace IqSoft.CP.Integration.Payments.Models.Paylado
{
	public class ResultOutput
	{
		public string ResultStatus { get; set; }
		public string ResultCode { get; set; }
		public string ResultMessage { get; set; }
		public string TransactionId { get; set; }
		public string TransactionType { get; set; }
		public string TransactionReference { get; set; }
		public string TransactionDate { get; set; }
		public string Amount { get; set; }
		public string Currency { get; set; }
		public string TransactionUrl { get; set; }
		public string SettlementDate { get; set; }
		public string IpAddress { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string PaymentType { get; set; }
		public string PaymentOptionAlias { get; set; }
		public string AccountHolder { get; set; }
		public string PayladoId { get; set; }
		public string Token { get; set; }
		public int HasChargeback { get; set; }
		public string MaxAttempts { get; set; }
		public string Attempt { get; set; }
	}
}
