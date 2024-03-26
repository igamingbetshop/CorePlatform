namespace IqSoft.CP.PaymentGateway.Models.Yaspa
{
	public class PaymentInput
	{
		public string type { get; set; }
		public object data { get; set; }
	}

	public class Data
	{
		public string citizenTransactionId { get; set; }
		public string customerIdentifier { get; set; }
		public string transactionType { get; set; }
		public string transactionStatus { get; set; }
		public string reference { get; set; }
		public string paymentAmount { get; set; }
		public string paymentCurrency { get; set; }
	}
}