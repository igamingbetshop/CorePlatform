namespace IqSoft.CP.Integration.Payments.Models.Xprizo
{
	public class PaymentInput
	{
		public long? accountId { get; set; }
		public string reference { get; set; }
		public decimal amount { get; set; }
		public string customer { get; set; }
		public string routingCode { get; set; }
		public string currencyCode { get; set; }
		public string redirect { get; set; }
		public long? fromAccountId { get; set; }
		public long? toAccountId { get; set; }
		public string mobileNumber { get; set; }
		public string description { get; set; }
	}
}
