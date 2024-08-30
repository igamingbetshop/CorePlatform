namespace IqSoft.CP.Integration.Payments.Models.Katarun
{
	public class PayoutClient
	{
		public string email { get; set; }
		public string phone { get; set; }
	}

	public class Payment
	{
		public string amount { get; set; }
		public string currency { get; set; }
	}

	public class PayoutInput
	{
		public PayoutClient client { get; set; }
		public Payment payment { get; set; }
		public string brand_id { get; set; }
		public string reference { get; set; }
	}
}

