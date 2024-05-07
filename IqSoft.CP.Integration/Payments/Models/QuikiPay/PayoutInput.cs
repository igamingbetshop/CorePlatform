namespace IqSoft.CP.Integration.Payments.Models.QuikiPay
{
	public class PayoutInput
	{
		public string type { get; set; }
		public string source { get; set; }
		public string currency { get; set; }
		public string amount { get; set; }
		public string end_user_ip { get; set; }
		public string crypto_currency { get; set; }
		public string crypto_address { get; set; }
		public string withdrawal_id { get; set; }
		public string customer_name { get; set; }
		public string customer_email { get; set; }
		public string is_local { get; set; }
		public string callback_url { get; set; }
		public int auto { get; set; }
		public string signature { get; set; }
	}
}
