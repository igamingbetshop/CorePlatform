namespace IqSoft.CP.Integration.Payments.Models.Huch
{
	public class AuthOutput
	{
		public string access_token { get; set; }
		public string expires_in { get; set; }
		public string token_type { get; set; }
	}

	public class PaymentOutput
	{
		public string payment_status { get; set; }
		public string status_info { get; set; }
		public string payment_id { get; set; }
		public string type { get; set; }
		public string status { get; set; }
		public string currency { get; set; }
		public int amount { get; set; }
		public string merchant_id { get; set; }
		public string url { get; set; }
		public string order_ref { get; set; }
		public string redirect_url { get; set; }
	}
}
