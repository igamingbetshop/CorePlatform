namespace IqSoft.CP.Integration.Payments.Models.Yaspa
{
	public class PayoutOutput
	{
		public string citizenTransactionId { get; set; }
		public string merchantId { get; set; }
		public string merchantTradingName { get; set; }
		public string customerIdentifier { get; set; }
		public string transactionStatus { get; set; }
		public long creationDate { get; set; }
		public string citizenCounterPartyId { get; set; }
		public string providerPaymentId { get; set; }
		public string currency { get; set; }
		public string paymentGiro { get; set; }
		public string merchantBankCode { get; set; }
		public string merchantAccountNumber { get; set; }
		public string merchantBank { get; set; }
		public string counterPartyBank { get; set; }
		public string counterPartyBankCode { get; set; }
		public string counterPartyAccountNumber { get; set; }
		public string counterPartyAccountName { get; set; }
		public double amount { get; set; }
		public string reference { get; set; }
	}
}
