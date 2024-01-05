namespace IqSoft.CP.Integration.Payments.Models.JetonHavale
{
	public class PaymentInput
	{
		public Auth auth { get; set; }
		public Customer customer { get; set; }
		public string transactionId { get; set; }
		public string returnUrl { get; set; }
		public decimal amount { get; set; }
		public string iban { get; set; }
		public string bank { get; set; }
	}
	public class Auth
	{
		public string apiKey { get; set; }
		public string secKey { get; set; }
	}

	public class Customer
	{
		public string id { get; set; }
		public string username { get; set; }
		public string fullName { get; set; }
	}
}
